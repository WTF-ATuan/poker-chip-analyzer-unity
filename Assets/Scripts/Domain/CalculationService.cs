using UnityEngine;
using System.Collections.Generic;
using PokerChipAnalyzer.Detection;

namespace PokerChipAnalyzer.Domain
{
    /// <summary>
    /// Service for calculating chip values and totals
    /// </summary>
    public class CalculationService : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ChipDenominationConfig chipConfig;
        [SerializeField] private BlindSettings blindSettings;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Events
        public System.Action<CalculationResult> OnCalculationComplete;
        
        // Properties
        public ChipDenominationConfig ChipConfig 
        { 
            get => chipConfig; 
            set => chipConfig = value; 
        }
        
        public BlindSettings BlindSettings 
        { 
            get => blindSettings; 
            set => blindSettings = value; 
        }
        
        private void Start()
        {
            InitializeService();
        }
        
        /// <summary>
        /// Initialize the calculation service
        /// </summary>
        private void InitializeService()
        {
            if (chipConfig == null)
            {
                chipConfig = ChipDenominationConfig.CreateDefault();
            }
            
            if (blindSettings == null)
            {
                blindSettings = BlindSettings.CreateDefault();
            }
            
            if (enableDebugLogs)
                Debug.Log("[CalculationService] Initialized");
        }
        
        /// <summary>
        /// Calculate total value from stack regions
        /// </summary>
        /// <param name="regions">Array of regions</param>
        /// <param name="chipCounts">Array of chip counts per region</param>
        /// <param name="chipColors">Array of chip colors per region</param>
        /// <returns>Calculation result</returns>
        public CalculationResult CalculateTotal(Rect[] regions, int[] chipCounts, Color[] chipColors)
        {
            if (regions.Length != chipCounts.Length || regions.Length != chipColors.Length)
            {
                Debug.LogError("[CalculationService] Array length mismatch");
                return new CalculationResult { isValid = false };
            }
            
            var result = new CalculationResult
            {
                isValid = true,
                stacks = new List<ChipStack>(),
                totalValue = 0f,
                totalBB = 0f,
                confidence = 1f
            };
            
            float totalConfidence = 0f;
            int validStacks = 0;
            
            for (int i = 0; i < regions.Length; i++)
            {
                var stack = CalculateStackValue(chipCounts[i], chipColors[i]);
                if (stack.isValid)
                {
                    result.stacks.Add(stack);
                    result.totalValue += stack.value;
                    totalConfidence += stack.confidence;
                    validStacks++;
                }
            }
            
            if (validStacks > 0)
            {
                result.totalBB = blindSettings.DollarsToBB(result.totalValue);
                result.confidence = totalConfidence / validStacks;
            }
            
            OnCalculationComplete?.Invoke(result);
            
            if (enableDebugLogs)
            {
                Debug.Log($"[CalculationService] Total: {blindSettings.FormatValue(result.totalValue)}, " +
                         $"Confidence: {result.confidence:F2}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Calculate value for a single stack
        /// </summary>
        /// <param name="chipCount">Number of chips</param>
        /// <param name="chipColor">Color of chips</param>
        /// <returns>Chip stack result</returns>
        public ChipStack CalculateStackValue(int chipCount, Color chipColor)
        {
            var denomination = chipConfig.GetDenominationByColor(chipColor);
            
            if (denomination == null)
            {
                return new ChipStack
                {
                    isValid = false,
                    chipCount = chipCount,
                    denomination = null,
                    value = 0f,
                    confidence = 0f
                };
            }
            
            float value = chipCount * denomination.value;
            float confidence = CalculateStackConfidence(chipCount, denomination);
            
            return new ChipStack
            {
                isValid = true,
                chipCount = chipCount,
                denomination = denomination,
                value = value,
                confidence = confidence
            };
        }
        
        /// <summary>
        /// Calculate confidence for a stack
        /// </summary>
        /// <param name="chipCount">Number of chips</param>
        /// <param name="denomination">Chip denomination</param>
        /// <returns>Confidence value (0-1)</returns>
        private float CalculateStackConfidence(int chipCount, ChipDenomination denomination)
        {
            float confidence = 1f;
            
            // Reduce confidence for unusual stack sizes
            int typicalSize = denomination.typicalStackSize;
            float sizeRatio = (float)chipCount / typicalSize;
            
            if (sizeRatio < 0.5f || sizeRatio > 2f)
            {
                confidence *= 0.7f; // Reduce confidence for unusual sizes
            }
            
            // Reduce confidence for very large stacks
            if (chipCount > typicalSize * 3)
            {
                confidence *= 0.5f;
            }
            
            return Mathf.Clamp01(confidence);
        }
        
        /// <summary>
        /// Get error margin for total value
        /// </summary>
        /// <param name="totalValue">Total value</param>
        /// <param name="confidence">Overall confidence</param>
        /// <returns>Error margin in dollars</returns>
        public float GetErrorMargin(float totalValue, float confidence)
        {
            // Error margin increases with lower confidence
            float errorPercent = (1f - confidence) * 0.1f; // Up to 10% error
            return totalValue * errorPercent;
        }
        
        /// <summary>
        /// Format calculation result for display
        /// </summary>
        /// <param name="result">Calculation result</param>
        /// <returns>Formatted string</returns>
        public string FormatResult(CalculationResult result)
        {
            if (!result.isValid)
                return "Invalid calculation";
            
            string formatted = blindSettings.FormatValue(result.totalValue);
            
            if (result.confidence < 0.8f)
            {
                float errorMargin = GetErrorMargin(result.totalValue, result.confidence);
                formatted += $" (±${errorMargin:F0})";
            }
            
            return formatted;
        }
    }
    
    /// <summary>
    /// Result of chip stack calculation
    /// </summary>
    [System.Serializable]
    public class ChipStack
    {
        public bool isValid;
        public int chipCount;
        public ChipDenomination denomination;
        public float value;
        public float confidence;
    }
    
    /// <summary>
    /// Result of total calculation
    /// </summary>
    [System.Serializable]
    public class CalculationResult
    {
        public bool isValid;
        public List<ChipStack> stacks;
        public float totalValue;
        public float totalBB;
        public float confidence;
    }
}
