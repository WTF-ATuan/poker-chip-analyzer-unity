using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace PokerChipAnalyzer.AR
{
    /// <summary>
    /// Provides access to depth data from LiDAR and camera
    /// </summary>
    public class DepthProvider : MonoBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private ARCameraManager arCameraManager;
        [SerializeField] private ARMeshManager arMeshManager;
        
        [Header("Settings")]
        [SerializeField] private float maxDepthDistance = 2.0f; // 2 meters max
        [SerializeField] private int depthSampleRate = 30; // Hz
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Events
        public System.Action<Texture2D> OnDepthTextureUpdated;
        public System.Action<Mesh> OnMeshUpdated;
        
        // Properties
        public bool IsDepthAvailable => arCameraManager != null && arCameraManager.GetComponent<Camera>() != null;
        public bool IsLiDARAvailable => arMeshManager != null && arMeshManager.subsystem != null;
        
        private float lastDepthUpdateTime;
        private Texture2D currentDepthTexture;
        
        private void Start()
        {
            InitializeDepthProvider();
        }
        
        private void Update()
        {
            if (Time.time - lastDepthUpdateTime >= 1.0f / depthSampleRate)
            {
                UpdateDepthData();
                lastDepthUpdateTime = Time.time;
            }
        }
        
        /// <summary>
        /// Initialize depth provider components
        /// </summary>
        private void InitializeDepthProvider()
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[DepthProvider] LiDAR Available: {IsLiDARAvailable}");
                Debug.Log($"[DepthProvider] Depth Available: {IsDepthAvailable}");
            }
        }
        
        /// <summary>
        /// Update depth data from AR camera
        /// </summary>
        private void UpdateDepthData()
        {
            if (!IsDepthAvailable) return;
            
            // Get depth texture from AR camera
            var camera = arCameraManager.GetComponent<Camera>();
            if (camera != null)
            {
                // Note: Camera.depthTexture is not directly accessible in AR
                // This would need to be implemented using AR camera depth data
                // For now, we'll skip this implementation
                if (enableDebugLogs)
                    Debug.Log("[DepthProvider] Depth texture access not implemented for AR");
            }
        }
        
        /// <summary>
        /// Get depth value at screen coordinates
        /// </summary>
        /// <param name="screenPoint">Screen coordinates (0-1 normalized)</param>
        /// <returns>Depth in meters, or -1 if unavailable</returns>
        public float GetDepthAtPoint(Vector2 screenPoint)
        {
            if (!IsDepthAvailable || currentDepthTexture == null)
                return -1f;
                
            // Convert normalized screen coordinates to texture coordinates
            int x = Mathf.RoundToInt(screenPoint.x * currentDepthTexture.width);
            int y = Mathf.RoundToInt(screenPoint.y * currentDepthTexture.height);
            
            x = Mathf.Clamp(x, 0, currentDepthTexture.width - 1);
            y = Mathf.Clamp(y, 0, currentDepthTexture.height - 1);
            
            // Sample depth texture
            Color depthColor = currentDepthTexture.GetPixel(x, y);
            float depth = depthColor.r; // Assuming depth is stored in red channel
            
            // Convert to meters and clamp
            depth = Mathf.Clamp(depth, 0f, maxDepthDistance);
            
            return depth;
        }
        
        /// <summary>
        /// Get average depth in a region
        /// </summary>
        /// <param name="center">Center point (0-1 normalized)</param>
        /// <param name="radius">Radius in normalized coordinates</param>
        /// <returns>Average depth in meters</returns>
        public float GetAverageDepthInRegion(Vector2 center, float radius)
        {
            if (!IsDepthAvailable || currentDepthTexture == null)
                return -1f;
                
            float totalDepth = 0f;
            int sampleCount = 0;
            
            int centerX = Mathf.RoundToInt(center.x * currentDepthTexture.width);
            int centerY = Mathf.RoundToInt(center.y * currentDepthTexture.height);
            int radiusPixels = Mathf.RoundToInt(radius * currentDepthTexture.width);
            
            // Sample in a circular region
            for (int x = centerX - radiusPixels; x <= centerX + radiusPixels; x++)
            {
                for (int y = centerY - radiusPixels; y <= centerY + radiusPixels; y++)
                {
                    if (x >= 0 && x < currentDepthTexture.width && 
                        y >= 0 && y < currentDepthTexture.height)
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                        if (distance <= radiusPixels)
                        {
                            Color depthColor = currentDepthTexture.GetPixel(x, y);
                            float depth = Mathf.Clamp(depthColor.r, 0f, maxDepthDistance);
                            totalDepth += depth;
                            sampleCount++;
                        }
                    }
                }
            }
            
            return sampleCount > 0 ? totalDepth / sampleCount : -1f;
        }
        
        /// <summary>
        /// Get LiDAR mesh data
        /// </summary>
        /// <returns>Latest mesh from LiDAR, or null if unavailable</returns>
        public Mesh GetLiDARMesh()
        {
            if (!IsLiDARAvailable) return null;
            
            // Get the latest mesh from ARMeshManager
            var meshes = arMeshManager.meshes;
            if (meshes != null && meshes.Count > 0)
            {
                return meshes[meshes.Count - 1].mesh;
            }
            
            return null;
        }
    }
}
