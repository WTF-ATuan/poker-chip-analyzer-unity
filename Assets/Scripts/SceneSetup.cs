using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
            // Create main UI Canvas
            GameObject canvasGO = GameObject.Find("Main UI Canvas");
            if (canvasGO == null)
            {
                canvasGO = new GameObject("Main UI Canvas");
                Canvas canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            
            // Create HUD Panel
            GameObject hudPanelGO = GameObject.Find("HUD Panel");
            if (hudPanelGO == null)
            {
                hudPanelGO = new GameObject("HUD Panel");
                hudPanelGO.transform.SetParent(canvasGO.transform);
                
                RectTransform hudRect = hudPanelGO.AddComponent<RectTransform>();
                hudRect.anchorMin = new Vector2(0f, 0f);
                hudRect.anchorMax = new Vector2(1f, 1f);
                hudRect.offsetMin = Vector2.zero;
                hudRect.offsetMax = Vector2.zero;
            }
            
            // Create scanning area overlay
            CreateScanningArea(hudPanelGO);
            
            // Create control buttons
            CreateControlButtons(hudPanelGO);
            
            // Create status display
            CreateStatusDisplay(hudPanelGO);
            
            // Create results panel
            CreateResultsPanel(hudPanelGO);
            
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
        /// Create scanning area overlay
        /// </summary>
        private void CreateScanningArea(GameObject parent)
        {
            GameObject scanningAreaGO = new GameObject("Scanning Area");
            scanningAreaGO.transform.SetParent(parent.transform);
            
            RectTransform scanningRect = scanningAreaGO.AddComponent<RectTransform>();
            scanningRect.anchorMin = new Vector2(0.1f, 0.1f);
            scanningRect.anchorMax = new Vector2(0.9f, 0.9f);
            scanningRect.offsetMin = Vector2.zero;
            scanningRect.offsetMax = Vector2.zero;
            
            // Add border image
            Image borderImage = scanningAreaGO.AddComponent<Image>();
            borderImage.color = new Color(0f, 1f, 0f, 0.3f); // Semi-transparent green
            
            // Create corner markers
            CreateCornerMarkers(scanningAreaGO);
        }
        
        /// <summary>
        /// Create corner markers for scanning area
        /// </summary>
        private void CreateCornerMarkers(GameObject parent)
        {
            Vector2[] corners = {
                new Vector2(0f, 1f), // Top-left
                new Vector2(1f, 1f), // Top-right
                new Vector2(0f, 0f), // Bottom-left
                new Vector2(1f, 0f)  // Bottom-right
            };
            
            for (int i = 0; i < corners.Length; i++)
            {
                GameObject cornerGO = new GameObject($"Corner_{i}");
                cornerGO.transform.SetParent(parent.transform);
                
                Image cornerImage = cornerGO.AddComponent<Image>();
                cornerImage.color = Color.green;
                
                RectTransform cornerRect = cornerGO.GetComponent<RectTransform>();
                cornerRect.anchorMin = corners[i];
                cornerRect.anchorMax = corners[i];
                cornerRect.sizeDelta = new Vector2(20f, 20f);
                cornerRect.anchoredPosition = Vector2.zero;
            }
        }
        
        /// <summary>
        /// Create control buttons
        /// </summary>
        private void CreateControlButtons(GameObject parent)
        {
            GameObject buttonPanelGO = new GameObject("Control Buttons");
            buttonPanelGO.transform.SetParent(parent.transform);
            
            RectTransform buttonRect = buttonPanelGO.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0f, 0f);
            buttonRect.anchorMax = new Vector2(1f, 0.15f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            // Add background
            Image buttonBg = buttonPanelGO.AddComponent<Image>();
            buttonBg.color = new Color(0f, 0f, 0f, 0.7f);
            
            // Create buttons
            CreateButton(buttonPanelGO, "Start Scan", new Vector2(0.1f, 0.5f), () => {
                Debug.Log("[SceneSetup] Start Scan clicked");
            });
            
            CreateButton(buttonPanelGO, "Add Region", new Vector2(0.3f, 0.5f), () => {
                var selector = FindFirstObjectByType<StackRegionSelector>();
                if (selector != null) selector.StartRegionSelection();
            });
            
            CreateButton(buttonPanelGO, "Clear All", new Vector2(0.5f, 0.5f), () => {
                var selector = FindFirstObjectByType<StackRegionSelector>();
                if (selector != null) selector.ClearAllRegions();
            });
            
            CreateButton(buttonPanelGO, "Calculate", new Vector2(0.7f, 0.5f), () => {
                Debug.Log("[SceneSetup] Calculate clicked");
            });
        }
        
        /// <summary>
        /// Create status display
        /// </summary>
        private void CreateStatusDisplay(GameObject parent)
        {
            GameObject statusGO = new GameObject("Status Display");
            statusGO.transform.SetParent(parent.transform);
            
            RectTransform statusRect = statusGO.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 0.85f);
            statusRect.anchorMax = new Vector2(1f, 1f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
            
            // Add background
            Image statusBg = statusGO.AddComponent<Image>();
            statusBg.color = new Color(0f, 0f, 0f, 0.5f);
            
            // Add status text
            GameObject statusTextGO = new GameObject("Status Text");
            statusTextGO.transform.SetParent(statusGO.transform);
            
            TextMeshProUGUI statusText = statusTextGO.AddComponent<TextMeshProUGUI>();
            statusText.text = "Ready to scan poker chips";
            statusText.fontSize = 18;
            statusText.color = Color.white;
            statusText.alignment = TextAlignmentOptions.Center;
            
            RectTransform statusTextRect = statusTextGO.GetComponent<RectTransform>();
            statusTextRect.anchorMin = Vector2.zero;
            statusTextRect.anchorMax = Vector2.one;
            statusTextRect.offsetMin = Vector2.zero;
            statusTextRect.offsetMax = Vector2.zero;
        }
        
        /// <summary>
        /// Create results panel
        /// </summary>
        private void CreateResultsPanel(GameObject parent)
        {
            GameObject resultsGO = new GameObject("Results Panel");
            resultsGO.transform.SetParent(parent.transform);
            
            RectTransform resultsRect = resultsGO.AddComponent<RectTransform>();
            resultsRect.anchorMin = new Vector2(0.7f, 0.2f);
            resultsRect.anchorMax = new Vector2(1f, 0.8f);
            resultsRect.offsetMin = Vector2.zero;
            resultsRect.offsetMax = Vector2.zero;
            
            // Add background
            Image resultsBg = resultsGO.AddComponent<Image>();
            resultsBg.color = new Color(0f, 0f, 0f, 0.8f);
            
            // Add results text
            GameObject resultsTextGO = new GameObject("Results Text");
            resultsTextGO.transform.SetParent(resultsGO.transform);
            
            TextMeshProUGUI resultsText = resultsTextGO.AddComponent<TextMeshProUGUI>();
            resultsText.text = "Results will appear here";
            resultsText.fontSize = 14;
            resultsText.color = Color.white;
            resultsText.alignment = TextAlignmentOptions.TopLeft;
            
            RectTransform resultsTextRect = resultsTextGO.GetComponent<RectTransform>();
            resultsTextRect.anchorMin = Vector2.zero;
            resultsTextRect.anchorMax = Vector2.one;
            resultsTextRect.offsetMin = new Vector2(10f, 10f);
            resultsTextRect.offsetMax = new Vector2(-10f, -10f);
        }
        
        /// <summary>
        /// Create a button with callback
        /// </summary>
        private void CreateButton(GameObject parent, string text, Vector2 anchorPos, System.Action onClick)
        {
            GameObject buttonGO = new GameObject($"Button_{text}");
            buttonGO.transform.SetParent(parent.transform);
            
            Button button = buttonGO.AddComponent<Button>();
            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 1f, 0.8f);
            
            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchorPos;
            buttonRect.anchorMax = anchorPos + new Vector2(0.15f, 0.3f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);
            TextMeshProUGUI buttonText = textGO.AddComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 12;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            button.onClick.AddListener(() => onClick());
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
            ARSessionController[] controllers = FindObjectsByType<ARSessionController>(FindObjectsSortMode.None);
            foreach (var controller in controllers)
            {
                DestroyImmediate(controller.gameObject);
            }
            
            if (enableDebugLogs)
                Debug.Log("[SceneSetup] Scene cleaned");
        }
    }
}
