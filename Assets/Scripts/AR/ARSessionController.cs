using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace PokerChipAnalyzer.AR
{
    /// <summary>
    /// Controls AR session lifecycle and provides access to AR subsystems
    /// </summary>
    public class ARSessionController : MonoBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private ARSession arSession;
        [SerializeField] private GameObject xrOrigin;
        [SerializeField] private ARCameraManager arCameraManager;
        
        [Header("AR Managers")]
        [SerializeField] private ARPlaneManager arPlaneManager;
        [SerializeField] private ARPointCloudManager arPointCloudManager;
        [SerializeField] private ARMeshManager arMeshManager;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Events
        public System.Action OnARSessionStarted;
        public System.Action OnARSessionStopped;
        public System.Action<string> OnARSessionError;
        
        // Properties
        public bool IsSessionRunning => arSession != null && ARSession.state == ARSessionState.SessionTracking;
        public GameObject XROrigin => xrOrigin;
        public ARCameraManager CameraManager => arCameraManager;
        
        private void Start()
        {
            InitializeAR();
        }
        
        private void OnDestroy()
        {
            StopAR();
        }
        
        /// <summary>
        /// Initialize AR session and subsystems
        /// </summary>
        public void InitializeAR()
        {
            if (enableDebugLogs)
                Debug.Log("[ARSessionController] Initializing AR...");
                
            // Start AR session
            if (arSession != null)
            {
                arSession.enabled = true;
            }
            
            // Enable plane detection for table detection
            if (arPlaneManager != null)
            {
                arPlaneManager.enabled = true;
                arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            }
            
            // Enable point cloud for depth data
            if (arPointCloudManager != null)
            {
                arPointCloudManager.enabled = true;
            }
            
            // Enable mesh generation for LiDAR data
            if (arMeshManager != null)
            {
                arMeshManager.enabled = true;
            }
            
            OnARSessionStarted?.Invoke();
        }
        
        /// <summary>
        /// Stop AR session and cleanup
        /// </summary>
        public void StopAR()
        {
            if (enableDebugLogs)
                Debug.Log("[ARSessionController] Stopping AR...");
                
            if (arSession != null)
            {
                arSession.enabled = false;
            }
            
            OnARSessionStopped?.Invoke();
        }
        
        /// <summary>
        /// Check if LiDAR is available on this device
        /// </summary>
        public bool IsLiDARAvailable()
        {
            return arMeshManager != null && arMeshManager.subsystem != null && arMeshManager.subsystem.running;
        }
        
        /// <summary>
        /// Get current AR session state
        /// </summary>
        public ARSessionState GetSessionState()
        {
            return arSession != null ? ARSession.state : ARSessionState.None;
        }
    }
}
