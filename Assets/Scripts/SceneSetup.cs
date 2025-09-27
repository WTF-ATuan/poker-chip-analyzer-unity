using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using PokerChipAnalyzer.AR;
using PokerChipAnalyzer.Detection;
using PokerChipAnalyzer.Domain;
using PokerChipAnalyzer.UI;
using PokerChipAnalyzer.IAP;

namespace PokerChipAnalyzer
{
    /// <summary>
    /// Sets up the Main scene with all required AR components
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("AR Components")]
        [SerializeField] private GameObject arSessionOriginPrefab;
        [SerializeField] private GameObject arCameraPrefab;
        
        [Header("UI Components")]
        [SerializeField] private GameObject hudCanvasPrefab;
        [SerializeField] private GameObject resultsCanvasPrefab;
        
        [Header("Debug")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private bool enableDebugLogs = true;
        
        private void Start()
        {
            if (autoSetup)
            {
                SetupScene();
            }
        }
        
        /// <summary>
        /// Setup the scene with all required components
        /// </summary>
        [ContextMenu("Setup Scene")]
        public void SetupScene()
        {
            if (enableDebugLogs)
                Debug.Log("[SceneSetup] Starting scene setup...");
            
            // 1. Setup XR Origin
            SetupXROrigin();
            
            // 2. Setup AR Camera
            SetupARCamera();
            
            // 3. Setup AR Managers
            SetupARManagers();
            
            // 4. Setup Core Components
            SetupCoreComponents();
            
            // 5. Setup UI
            SetupUI();
            
            // 6. Setup IAP
            SetupIAP();
            
            if (enableDebugLogs)
                Debug.Log("[SceneSetup] Scene setup complete!");
        }
        
        /// <summary>
        /// Setup XR Origin (replaces deprecated ARSessionOrigin)
        /// </summary>
        private void SetupXROrigin()
        {
            GameObject xrOriginGO = GameObject.Find("XR Origin");
            if (xrOriginGO == null)
            {
                xrOriginGO = new GameObject("XR Origin");
            }
            
            // Note: In Unity 6000, we'll use ARSessionOrigin for now
            // XROrigin integration will be added when XR Interaction Toolkit is available
            // For now, we'll create a simple GameObject as the origin
            
            // Add ARSessionController
            ARSessionController arController = xrOriginGO.GetComponent<ARSessionController>();
            if (arController == null)
            {
                arController = xrOriginGO.AddComponent<ARSessionController>();
            }
            
            if (enableDebugLogs)
                Debug.Log("[SceneSetup] XR Origin setup complete");
        }
        
        /// <summary>
        /// Setup AR Camera
        /// </summary>
        private void SetupARCamera()
        {
            GameObject arCameraGO = GameObject.Find("AR Camera");
            if (arCameraGO == null)
            {
                arCameraGO = new GameObject("AR Camera");
            }
            
            // Add ARCameraManager component
            ARCameraManager arCameraManager = arCameraGO.GetComponent<ARCameraManager>();
            if (arCameraManager == null)
            {
                arCameraManager = arCameraGO.AddComponent<ARCameraManager>();
            }
            
            // Add Camera component
            Camera camera = arCameraGO.GetComponent<Camera>();
            if (camera == null)
            {
                camera = arCameraGO.AddComponent<Camera>();
            }
            
            // Configure camera
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 20f;
            
            // Add DepthProvider
            DepthProvider depthProvider = arCameraGO.GetComponent<DepthProvider>();
            if (depthProvider == null)
            {
                depthProvider = arCameraGO.AddComponent<DepthProvider>();
            }
            
            // Parent to XR Origin
            GameObject xrOriginGO = GameObject.Find("XR Origin");
            if (xrOriginGO != null)
            {
                arCameraGO.transform.SetParent(xrOriginGO.transform);
                arCameraGO.transform.localPosition = Vector3.zero;
                arCameraGO.transform.localRotation = Quaternion.identity;
            }
            
            if (enableDebugLogs)
                Debug.Log("[SceneSetup] AR Camera setup complete");
        }
        
        /// <summary>
        /// Setup AR Managers
        /// </summary>
        private void SetupARManagers()
        {
            GameObject xrOriginGO = GameObject.Find("XR Origin");
            if (xrOriginGO == null) return;
            
            // AR Session
            ARSession arSession = xrOriginGO.GetComponent<ARSession>();
            if (arSession == null)
            {
                arSession = xrOriginGO.AddComponent<ARSession>();
            }
            
            // AR Plane Manager
            ARPlaneManager arPlaneManager = xrOriginGO.GetComponent<ARPlaneManager>();
            if (arPlaneManager == null)
            {
                arPlaneManager = xrOriginGO.AddComponent<ARPlaneManager>();
            }
            
            // AR Point Cloud Manager
            ARPointCloudManager arPointCloudManager = xrOriginGO.GetComponent<ARPointCloudManager>();
            if (arPointCloudManager == null)
            {
                arPointCloudManager = xrOriginGO.AddComponent<ARPointCloudManager>();
            }
            
            // AR Mesh Manager (for LiDAR)
            ARMeshManager arMeshManager = xrOriginGO.GetComponent<ARMeshManager>();
            if (arMeshManager == null)
            {
                arMeshManager = xrOriginGO.AddComponent<ARMeshManager>();
            }
            
            if (enableDebugLogs)
                Debug.Log("[SceneSetup] AR Managers setup complete");
        }
        
        /// <summary>
        /// Setup core components
        /// </summary>
        private void SetupCoreComponents()
        {
            // Stack Region Selector
            GameObject selectorGO = GameObject.Find("Stack Region Selector");
            if (selectorGO == null)
            {
                selectorGO = new GameObject("Stack Region Selector");
            }
            
            StackRegionSelector selector = selectorGO.GetComponent<StackRegionSelector>();
            if (selector == null)
            {
                selector = selectorGO.AddComponent<StackRegionSelector>();
            }
            
            // Stack Height Estimator
            GameObject estimatorGO = GameObject.Find("Stack Height Estimator");
            if (estimatorGO == null)
            {
                estimatorGO = new GameObject("Stack Height Estimator");
            }
            
            StackHeightEstimator estimator = estimatorGO.GetComponent<StackHeightEstimator>();
            if (estimator == null)
            {
                estimator = estimatorGO.AddComponent<StackHeightEstimator>();
            }
            
            // Calculation Service
            GameObject calcGO = GameObject.Find("Calculation Service");
            if (calcGO == null)
            {
                calcGO = new GameObject("Calculation Service");
            }
            
            CalculationService calcService = calcGO.GetComponent<CalculationService>();
            if (calcService == null)
            {
                calcService = calcGO.AddComponent<CalculationService>();
            }
            
            if (enableDebugLogs)
                Debug.Log("[SceneSetup] Core components setup complete");
        }
        
        /// <summary>
        /// Setup UI components
        /// </summary>
        private void SetupUI()
        {
            // HUD Controller
            GameObject hudGO = GameObject.Find("HUD Controller");
            if (hudGO == null)
            {
                hudGO = new GameObject("HUD Controller");
            }
            
            HUDController hudController = hudGO.GetComponent<HUDController>();
            if (hudController == null)
            {
                hudController = hudGO.AddComponent<HUDController>();
            }
            
            // Results Panel
            GameObject resultsGO = GameObject.Find("Results Panel");
            if (resultsGO == null)
            {
                resultsGO = new GameObject("Results Panel");
            }
            
            ResultsPanel resultsPanel = resultsGO.GetComponent<ResultsPanel>();
            if (resultsPanel == null)
            {
                resultsPanel = resultsGO.AddComponent<ResultsPanel>();
            }
            
            if (enableDebugLogs)
                Debug.Log("[SceneSetup] UI components setup complete");
        }
        
        /// <summary>
        /// Setup IAP
        /// </summary>
        private void SetupIAP()
        {
            GameObject iapGO = GameObject.Find("IAP Manager");
            if (iapGO == null)
            {
                iapGO = new GameObject("IAP Manager");
            }
            
            IAPManager iapManager = iapGO.GetComponent<IAPManager>();
            if (iapManager == null)
            {
                iapManager = iapGO.AddComponent<IAPManager>();
            }
            
            // Make persistent
            DontDestroyOnLoad(iapGO);
            
            if (enableDebugLogs)
                Debug.Log("[SceneSetup] IAP setup complete");
        }
        
        /// <summary>
        /// Clean up scene (for testing)
        /// </summary>
        [ContextMenu("Clean Scene")]
        public void CleanScene()
        {
            // Remove all AR components
            GameObject xrOriginGO = GameObject.Find("XR Origin");
            if (xrOriginGO != null)
            {
                DestroyImmediate(xrOriginGO);
            }
            
            // Remove all custom components
            ARSessionController[] controllers = FindObjectsOfType<ARSessionController>();
            foreach (var controller in controllers)
            {
                DestroyImmediate(controller.gameObject);
            }
            
            if (enableDebugLogs)
                Debug.Log("[SceneSetup] Scene cleaned");
        }
    }
}
