using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PokerChipAnalyzer.Detection;
using PokerChipAnalyzer.Tests;

namespace PokerChipAnalyzer.Tests
{
    /// <summary>
    /// Quick setup for photo testing in Unity Editor
    /// </summary>
    public class PhotoTestSetup : MonoBehaviour
    {
        [Header("Quick Setup")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private bool enablePhotoMode = true;
        [SerializeField] private bool createNewUI = true; // 是否創建新 UI
        [SerializeField] private bool bindExistingButtons = false; // 是否綁定現有按鈕
        
        [Header("Manual Button Binding")]
        [SerializeField] private Button addRegionButton;
        [SerializeField] private Button clearAllButton;
        [SerializeField] private Button testHeightButton;
        [SerializeField] private Button nextPhotoButton;
        
        private void Start()
        {
            if (autoSetup)
            {
                if (createNewUI)
                {
                    SetupPhotoTest();
                }
                else if (bindExistingButtons)
                {
                    BindExistingButtons();
                }
                else
                {
                    SetupComponents();
                }
            }
        }
        
        /// <summary>
        /// Bind manually assigned buttons
        /// </summary>
        [ContextMenu("Bind Manual Buttons")]
        public void BindManualButtons()
        {
            Debug.Log("[PhotoTestSetup] Binding manually assigned buttons...");
            
            // Bind Add Region button
            if (addRegionButton != null)
            {
                addRegionButton.onClick.RemoveAllListeners();
                addRegionButton.onClick.AddListener(() => {
                    var selector = FindFirstObjectByType<StackRegionSelector>();
                    if (selector != null) 
                    {
                        selector.StartRegionSelection();
                        Debug.Log("[PhotoTestSetup] Started region selection via manual button");
                    }
                });
                Debug.Log($"[PhotoTestSetup] Bound 'Add Region' to button: {addRegionButton.name}");
            }
            
            // Bind Clear All button
            if (clearAllButton != null)
            {
                clearAllButton.onClick.RemoveAllListeners();
                clearAllButton.onClick.AddListener(() => {
                    var selector = FindFirstObjectByType<StackRegionSelector>();
                    if (selector != null) 
                    {
                        selector.ClearAllRegions();
                        Debug.Log("[PhotoTestSetup] Cleared all regions via manual button");
                    }
                });
                Debug.Log($"[PhotoTestSetup] Bound 'Clear All' to button: {clearAllButton.name}");
            }
            
            // Bind Test Height button
            if (testHeightButton != null)
            {
                testHeightButton.onClick.RemoveAllListeners();
                testHeightButton.onClick.AddListener(() => {
                    TestHeightEstimation();
                });
                Debug.Log($"[PhotoTestSetup] Bound 'Test Height' to button: {testHeightButton.name}");
            }
            
            // Bind Next Photo button
            if (nextPhotoButton != null)
            {
                nextPhotoButton.onClick.RemoveAllListeners();
                nextPhotoButton.onClick.AddListener(() => {
                    var testManager = FindFirstObjectByType<PhotoTestManager>();
                    if (testManager != null) 
                    {
                        testManager.NextPhoto();
                        Debug.Log("[PhotoTestSetup] Switched to next photo via manual button");
                    }
                });
                Debug.Log($"[PhotoTestSetup] Bound 'Next Photo' to button: {nextPhotoButton.name}");
            }
            
            // Setup components
            SetupComponents();
            
            Debug.Log("[PhotoTestSetup] Manual button binding complete!");
        }
        
        /// <summary>
        /// Bind existing buttons without creating new UI
        /// </summary>
        [ContextMenu("Bind Existing Buttons")]
        public void BindExistingButtons()
        {
            Debug.Log("[PhotoTestSetup] Binding existing buttons...");
            
            // Find existing buttons by name or tag
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            
            foreach (Button button in allButtons)
            {
                string buttonName = button.name.ToLower();
                
                if (buttonName.Contains("add") && buttonName.Contains("region"))
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        var selector = FindFirstObjectByType<StackRegionSelector>();
                        if (selector != null) 
                        {
                            selector.StartRegionSelection();
                            Debug.Log("[PhotoTestSetup] Started region selection via existing button");
                        }
                    });
                    Debug.Log($"[PhotoTestSetup] Bound 'Add Region' to button: {button.name}");
                }
                else if (buttonName.Contains("clear") && buttonName.Contains("all"))
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        var selector = FindFirstObjectByType<StackRegionSelector>();
                        if (selector != null) 
                        {
                            selector.ClearAllRegions();
                            Debug.Log("[PhotoTestSetup] Cleared all regions via existing button");
                        }
                    });
                    Debug.Log($"[PhotoTestSetup] Bound 'Clear All' to button: {button.name}");
                }
                else if (buttonName.Contains("test") && buttonName.Contains("height"))
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        TestHeightEstimation();
                    });
                    Debug.Log($"[PhotoTestSetup] Bound 'Test Height' to button: {button.name}");
                }
                else if (buttonName.Contains("next") && buttonName.Contains("photo"))
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        var testManager = FindFirstObjectByType<PhotoTestManager>();
                        if (testManager != null) 
                        {
                            testManager.NextPhoto();
                            Debug.Log("[PhotoTestSetup] Switched to next photo via existing button");
                        }
                    });
                    Debug.Log($"[PhotoTestSetup] Bound 'Next Photo' to button: {button.name}");
                }
            }
            
            // Setup components
            SetupComponents();
            
            Debug.Log("[PhotoTestSetup] Button binding complete!");
        }
        
        /// <summary>
        /// Setup photo testing environment
        /// </summary>
        [ContextMenu("Setup Photo Test")]
        public void SetupPhotoTest()
        {
            // Create canvas for photo display
            GameObject canvasGO = new GameObject("Photo Test Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create photo display
            GameObject photoGO = new GameObject("Photo Display");
            photoGO.transform.SetParent(canvasGO.transform);
            RawImage photoImage = photoGO.AddComponent<RawImage>();
            RectTransform photoRect = photoGO.GetComponent<RectTransform>();
            photoRect.anchorMin = new Vector2(0.1f, 0.1f);
            photoRect.anchorMax = new Vector2(0.9f, 0.9f);
            photoRect.offsetMin = Vector2.zero;
            photoRect.offsetMax = Vector2.zero;
            
            // Create control panel
            GameObject controlPanelGO = new GameObject("Control Panel");
            controlPanelGO.transform.SetParent(canvasGO.transform);
            RectTransform controlRect = controlPanelGO.AddComponent<RectTransform>();
            controlRect.anchorMin = new Vector2(0f, 0f);
            controlRect.anchorMax = new Vector2(1f, 0.1f);
            controlRect.offsetMin = Vector2.zero;
            controlRect.offsetMax = Vector2.zero;
            
            // Add background to control panel
            Image controlBg = controlPanelGO.AddComponent<Image>();
            controlBg.color = new Color(0f, 0f, 0f, 0.7f);
            
            // Create buttons
            CreateButton(controlPanelGO, "Add Region", new Vector2(0.1f, 0.5f), () => {
                var selector = FindFirstObjectByType<StackRegionSelector>();
                if (selector != null) 
                {
                    selector.StartRegionSelection();
                    Debug.Log("[PhotoTestSetup] Started region selection");
                }
                else
                {
                    Debug.LogWarning("[PhotoTestSetup] StackRegionSelector not found");
                }
            });
            
            CreateButton(controlPanelGO, "Clear All", new Vector2(0.3f, 0.5f), () => {
                var selector = FindFirstObjectByType<StackRegionSelector>();
                if (selector != null) 
                {
                    selector.ClearAllRegions();
                    Debug.Log("[PhotoTestSetup] Cleared all regions");
                }
                else
                {
                    Debug.LogWarning("[PhotoTestSetup] StackRegionSelector not found");
                }
            });
            
            CreateButton(controlPanelGO, "Test Height", new Vector2(0.5f, 0.5f), () => {
                TestHeightEstimation();
            });
            
            CreateButton(controlPanelGO, "Next Photo", new Vector2(0.7f, 0.5f), () => {
                var testManager = FindFirstObjectByType<PhotoTestManager>();
                if (testManager != null) 
                {
                    testManager.NextPhoto();
                    Debug.Log("[PhotoTestSetup] Switched to next photo");
                }
                else
                {
                    Debug.LogWarning("[PhotoTestSetup] PhotoTestManager not found");
                }
            });
            
            // Create info text
            GameObject infoGO = new GameObject("Info Text");
            infoGO.transform.SetParent(controlPanelGO.transform);
            TextMeshProUGUI infoText = infoGO.AddComponent<TextMeshProUGUI>();
            infoText.text = "Photo Test Mode - Click 'Add Region' to select chip stack areas";
            infoText.fontSize = 16;
            infoText.color = Color.white;
            infoText.alignment = TextAlignmentOptions.Center;
            
            RectTransform infoRect = infoGO.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.1f, 0.1f);
            infoRect.anchorMax = new Vector2(0.9f, 0.9f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;
            
            // Setup components
            SetupComponents();
            
            Debug.Log("[PhotoTestSetup] Photo test environment created!");
        }
        
        /// <summary>
        /// Create selection box prefab for region selection
        /// </summary>
        /// <returns>Selection box prefab GameObject</returns>
        private GameObject CreateSelectionBoxPrefab()
        {
            GameObject selectionBoxGO = new GameObject("SelectionBoxPrefab");
            
            // Add Image component with hollow shader
            Image selectionImage = selectionBoxGO.AddComponent<Image>();
            
            // Try to load the custom shader
            Shader hollowShader = Shader.Find("Custom/SelectionBox");
            if (hollowShader != null)
            {
                Material hollowMaterial = new Material(hollowShader);
                hollowMaterial.SetColor("_Color", new Color(0f, 1f, 0f, 0.8f));
                hollowMaterial.SetFloat("_BorderWidth", 0.02f);
                selectionImage.material = hollowMaterial;
                Debug.Log("[PhotoTestSetup] Using hollow selection shader");
            }
            else
            {
                // Fallback to semi-transparent
                selectionImage.color = new Color(0f, 1f, 0f, 0.3f);
                Debug.LogWarning("[PhotoTestSetup] Hollow shader not found, using fallback");
            }
            
            // Add RectTransform
            RectTransform rectTransform = selectionBoxGO.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            Debug.Log("[PhotoTestSetup] Created hollow selection box prefab");
            return selectionBoxGO;
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
        /// Setup required components
        /// </summary>
        private void SetupComponents()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[PhotoTestSetup] Canvas not found! Make sure to call SetupPhotoTest() first.");
                return;
            }
            
            // Create StackRegionSelector if not exists
            StackRegionSelector selector = FindFirstObjectByType<StackRegionSelector>();
            if (selector == null)
            {
                GameObject selectorGO = new GameObject("StackRegionSelector");
                selector = selectorGO.AddComponent<StackRegionSelector>();
                Debug.Log("[PhotoTestSetup] Created StackRegionSelector");
            }
            
            // Setup canvas reference and selection box prefab for StackRegionSelector
            if (selector != null)
            {
                // Use reflection to set private field (for testing)
                var canvasField = typeof(StackRegionSelector).GetField("selectionCanvas", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (canvasField != null)
                {
                    canvasField.SetValue(selector, canvas.GetComponent<RectTransform>());
                    Debug.Log("[PhotoTestSetup] Set selectionCanvas for StackRegionSelector");
                }
                
                // Create selection box prefab
                GameObject selectionBoxPrefab = CreateSelectionBoxPrefab();
                
                var prefabField = typeof(StackRegionSelector).GetField("selectionBoxPrefab", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (prefabField != null)
                {
                    prefabField.SetValue(selector, selectionBoxPrefab.GetComponent<Image>());
                    Debug.Log("[PhotoTestSetup] Set selectionBoxPrefab for StackRegionSelector");
                }
            }
            
            // Create StackHeightEstimator if not exists
            StackHeightEstimator estimator = FindFirstObjectByType<StackHeightEstimator>();
            if (estimator == null)
            {
                GameObject estimatorGO = new GameObject("StackHeightEstimator");
                estimator = estimatorGO.AddComponent<StackHeightEstimator>();
                Debug.Log("[PhotoTestSetup] Created StackHeightEstimator");
            }
            
            // Enable simulated data for testing
            if (estimator != null)
            {
                var field = typeof(StackHeightEstimator).GetField("useSimulatedData", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(estimator, true);
                    Debug.Log("[PhotoTestSetup] Enabled simulated data for StackHeightEstimator");
                }
            }
            
            // Create PhotoTestManager if not exists
            PhotoTestManager testManager = FindFirstObjectByType<PhotoTestManager>();
            if (testManager == null)
            {
                GameObject testManagerGO = new GameObject("PhotoTestManager");
                testManager = testManagerGO.AddComponent<PhotoTestManager>();
                Debug.Log("[PhotoTestSetup] Created PhotoTestManager");
            }
            
            // Setup PhotoTestManager references
            if (testManager != null)
            {
                // Set photo display reference
                var photoDisplayField = typeof(PhotoTestManager).GetField("photoDisplay", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (photoDisplayField != null)
                {
                    RawImage photoImage = canvas.GetComponentInChildren<RawImage>();
                    if (photoImage != null)
                    {
                        photoDisplayField.SetValue(testManager, photoImage);
                        Debug.Log("[PhotoTestSetup] Set photoDisplay for PhotoTestManager");
                    }
                }
                
                // Set region selector reference
                var regionSelectorField = typeof(PhotoTestManager).GetField("regionSelector", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (regionSelectorField != null && selector != null)
                {
                    regionSelectorField.SetValue(testManager, selector);
                    Debug.Log("[PhotoTestSetup] Set regionSelector for PhotoTestManager");
                }
                
                // Set height estimator reference
                var heightEstimatorField = typeof(PhotoTestManager).GetField("heightEstimator", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (heightEstimatorField != null && estimator != null)
                {
                    heightEstimatorField.SetValue(testManager, estimator);
                    Debug.Log("[PhotoTestSetup] Set heightEstimator for PhotoTestManager");
                }
            }
            
            Debug.Log("[PhotoTestSetup] Component setup complete!");
        }
        
        /// <summary>
        /// Test height estimation with current regions
        /// </summary>
        private void TestHeightEstimation()
        {
            var selector = FindFirstObjectByType<StackRegionSelector>();
            var estimator = FindFirstObjectByType<StackHeightEstimator>();
            
            if (selector == null)
            {
                Debug.LogWarning("[PhotoTestSetup] StackRegionSelector not found");
                return;
            }
            
            if (estimator == null)
            {
                Debug.LogWarning("[PhotoTestSetup] StackHeightEstimator not found");
                return;
            }
            
            Rect[] regions = selector.GetAllRegions();
            if (regions.Length == 0)
            {
                Debug.LogWarning("[PhotoTestSetup] No regions selected. Click 'Add Region' first, then drag to select chip stack areas.");
                return;
            }
            
            Debug.Log($"[PhotoTestSetup] Testing {regions.Length} regions...");
            
            for (int i = 0; i < regions.Length; i++)
            {
                var result = estimator.EstimateHeight(regions[i]);
                Debug.Log($"[PhotoTestSetup] Region {i + 1}: " +
                         $"Height {result.heightMm:F1}mm, " +
                         $"Chips {result.chipCount}, " +
                         $"Confidence {result.confidence:F2}");
            }
        }
    }
}
