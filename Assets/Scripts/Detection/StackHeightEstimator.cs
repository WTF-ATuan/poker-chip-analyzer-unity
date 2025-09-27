using UnityEngine;
using PokerChipAnalyzer.AR;

namespace PokerChipAnalyzer.Detection
{
    /// <summary>
    /// Estimates chip stack height from depth data and calculates chip count
    /// </summary>
    public class StackHeightEstimator : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private DepthProvider depthProvider;
        
        [Header("Settings")]
        [SerializeField] private float chipThickness = 3.3f; // mm
        [SerializeField] private float smoothingFactor = 0.1f;
        [SerializeField] private int samplePoints = 9; // 3x3 grid
        [SerializeField] private float confidenceThreshold = 0.7f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Events
        public System.Action<float, float, float> OnHeightEstimated; // height, confidence, chipCount
        
        // Properties
        public float ChipThickness 
        { 
            get => chipThickness; 
            set => chipThickness = Mathf.Clamp(value, 3.0f, 4.0f); 
        }
        
        private System.Collections.Generic.Dictionary<Rect, HeightData> heightHistory = 
            new System.Collections.Generic.Dictionary<Rect, HeightData>();
        
        private struct HeightData
        {
            public float height;
            public float confidence;
            public float smoothedHeight;
            public int frameCount;
        }
        
        private void Start()
        {
            InitializeEstimator();
        }
        
        /// <summary>
        /// Initialize the height estimator
        /// </summary>
        private void InitializeEstimator()
        {
            if (depthProvider == null)
            {
                depthProvider = FindObjectOfType<DepthProvider>();
            }
            
            if (enableDebugLogs)
                Debug.Log("[StackHeightEstimator] Initialized");
        }
        
        /// <summary>
        /// Estimate height for a region
        /// </summary>
        /// <param name="region">Normalized screen region (0-1)</param>
        /// <returns>Height estimation result</returns>
        public HeightEstimationResult EstimateHeight(Rect region)
        {
            if (depthProvider == null)
            {
                return new HeightEstimationResult { isValid = false };
            }
            
            // Sample depth at multiple points in the region
            float[] depths = SampleDepthsInRegion(region);
            
            if (depths.Length == 0)
            {
                return new HeightEstimationResult { isValid = false };
            }
            
            // Calculate height and confidence
            float height = CalculateHeight(depths);
            float confidence = CalculateConfidence(depths);
            
            // Apply temporal smoothing
            HeightData historyData;
            if (heightHistory.TryGetValue(region, out historyData))
            {
                height = Mathf.Lerp(historyData.smoothedHeight, height, smoothingFactor);
                historyData.frameCount++;
            }
            else
            {
                historyData.frameCount = 1;
            }
            
            historyData.height = height;
            historyData.confidence = confidence;
            historyData.smoothedHeight = height;
            
            heightHistory[region] = historyData;
            
            // Calculate chip count
            int chipCount = Mathf.RoundToInt(height / chipThickness);
            chipCount = Mathf.Max(0, chipCount);
            
            var result = new HeightEstimationResult
            {
                isValid = true,
                heightMm = height,
                confidence = confidence,
                chipCount = chipCount,
                isConfident = confidence >= confidenceThreshold
            };
            
            OnHeightEstimated?.Invoke(height, confidence, chipCount);
            
            if (enableDebugLogs)
            {
                Debug.Log($"[StackHeightEstimator] Height: {height:F1}mm, Confidence: {confidence:F2}, Chips: {chipCount}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Sample depths at multiple points in the region
        /// </summary>
        /// <param name="region">Normalized region</param>
        /// <returns>Array of depth values</returns>
        private float[] SampleDepthsInRegion(Rect region)
        {
            System.Collections.Generic.List<float> depths = new System.Collections.Generic.List<float>();
            
            int samplesPerAxis = Mathf.RoundToInt(Mathf.Sqrt(samplePoints));
            float stepX = region.width / (samplesPerAxis - 1);
            float stepY = region.height / (samplesPerAxis - 1);
            
            for (int i = 0; i < samplesPerAxis; i++)
            {
                for (int j = 0; j < samplesPerAxis; j++)
                {
                    Vector2 samplePoint = new Vector2(
                        region.x + i * stepX,
                        region.y + j * stepY
                    );
                    
                    float depth = depthProvider.GetDepthAtPoint(samplePoint);
                    if (depth > 0) // Valid depth
                    {
                        depths.Add(depth);
                    }
                }
            }
            
            return depths.ToArray();
        }
        
        /// <summary>
        /// Calculate height from depth samples
        /// </summary>
        /// <param name="depths">Depth values in meters</param>
        /// <returns>Height in millimeters</returns>
        private float CalculateHeight(float[] depths)
        {
            if (depths.Length == 0) return 0f;
            
            // Find table plane (lowest depth values)
            System.Array.Sort(depths);
            
            // Use bottom 30% of samples as table reference
            int tableSampleCount = Mathf.Max(1, Mathf.RoundToInt(depths.Length * 0.3f));
            float tableDepth = 0f;
            
            for (int i = 0; i < tableSampleCount; i++)
            {
                tableDepth += depths[i];
            }
            tableDepth /= tableSampleCount;
            
            // Find stack top (highest depth values)
            int stackSampleCount = Mathf.Max(1, Mathf.RoundToInt(depths.Length * 0.3f));
            float stackTopDepth = 0f;
            
            for (int i = depths.Length - stackSampleCount; i < depths.Length; i++)
            {
                stackTopDepth += depths[i];
            }
            stackTopDepth /= stackSampleCount;
            
            // Calculate height difference
            float heightMeters = stackTopDepth - tableDepth;
            float heightMm = heightMeters * 1000f; // Convert to mm
            
            return Mathf.Max(0f, heightMm);
        }
        
        /// <summary>
        /// Calculate confidence based on depth variance and sample count
        /// </summary>
        /// <param name="depths">Depth values</param>
        /// <returns>Confidence value (0-1)</returns>
        private float CalculateConfidence(float[] depths)
        {
            if (depths.Length < 3) return 0f;
            
            // Calculate variance
            float mean = 0f;
            foreach (float depth in depths)
            {
                mean += depth;
            }
            mean /= depths.Length;
            
            float variance = 0f;
            foreach (float depth in depths)
            {
                float diff = depth - mean;
                variance += diff * diff;
            }
            variance /= depths.Length;
            
            float stdDev = Mathf.Sqrt(variance);
            
            // Confidence based on sample count and consistency
            float sampleConfidence = Mathf.Clamp01(depths.Length / (float)samplePoints);
            float consistencyConfidence = Mathf.Clamp01(1f - stdDev * 10f); // Lower std dev = higher confidence
            
            return (sampleConfidence + consistencyConfidence) * 0.5f;
        }
        
        /// <summary>
        /// Clear height history for a region
        /// </summary>
        /// <param name="region">Region to clear</param>
        public void ClearHistory(Rect region)
        {
            heightHistory.Remove(region);
        }
        
        /// <summary>
        /// Clear all height history
        /// </summary>
        public void ClearAllHistory()
        {
            heightHistory.Clear();
        }
        
        /// <summary>
        /// Get height estimation result
        /// </summary>
        [System.Serializable]
        public struct HeightEstimationResult
        {
            public bool isValid;
            public float heightMm;
            public float confidence;
            public int chipCount;
            public bool isConfident;
        }
    }
}
