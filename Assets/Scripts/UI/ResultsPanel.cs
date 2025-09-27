using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PokerChipAnalyzer.Domain;

namespace PokerChipAnalyzer.UI
{
    /// <summary>
    /// Displays calculation results and chip stack information
    /// </summary>
    public class ResultsPanel : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Canvas resultsCanvas;
        [SerializeField] private TextMeshProUGUI totalValueText;
        [SerializeField] private TextMeshProUGUI totalBBText;
        [SerializeField] private TextMeshProUGUI confidenceText;
        [SerializeField] private Transform stackListParent;
        [SerializeField] private GameObject stackItemPrefab;
        [SerializeField] private Button recalculateButton;
        [SerializeField] private Button exportButton;
        
        [Header("Settings")]
        [SerializeField] private Color highConfidenceColor = Color.green;
        [SerializeField] private Color lowConfidenceColor = Color.red;
        [SerializeField] private float confidenceThreshold = 0.7f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Events
        public System.Action OnRecalculateRequested;
        public System.Action OnExportRequested;
        
        // Properties
        public bool IsVisible => resultsCanvas != null && resultsCanvas.gameObject.activeInHierarchy;
        
        private CalculationService calculationService;
        
        private void Start()
        {
            InitializePanel();
        }
        
        /// <summary>
        /// Initialize results panel
        /// </summary>
        private void InitializePanel()
        {
            // Find calculation service
            calculationService = FindObjectOfType<CalculationService>();
            
            // Setup buttons
            if (recalculateButton != null)
                recalculateButton.onClick.AddListener(RequestRecalculate);
                
            if (exportButton != null)
                exportButton.onClick.AddListener(RequestExport);
            
            // Hide panel initially
            ShowPanel(false);
            
            if (enableDebugLogs)
                Debug.Log("[ResultsPanel] Initialized");
        }
        
        /// <summary>
        /// Display calculation results
        /// </summary>
        /// <param name="result">Calculation result</param>
        public void DisplayResults(CalculationResult result)
        {
            if (!result.isValid)
            {
                ShowError("Invalid calculation result");
                return;
            }
            
            // Update total values
            UpdateTotalValues(result);
            
            // Update confidence
            UpdateConfidence(result.confidence);
            
            // Update stack list
            UpdateStackList(result.stacks);
            
            // Show panel
            ShowPanel(true);
            
            if (enableDebugLogs)
                Debug.Log($"[ResultsPanel] Displayed results: {result.totalValue:F0} total");
        }
        
        /// <summary>
        /// Update total value displays
        /// </summary>
        /// <param name="result">Calculation result</param>
        private void UpdateTotalValues(CalculationResult result)
        {
            if (calculationService == null) return;
            
            string formattedValue = calculationService.FormatResult(result);
            
            if (totalValueText != null)
            {
                totalValueText.text = formattedValue;
            }
            
            if (totalBBText != null)
            {
                totalBBText.text = $"{result.totalBB:F1} BB";
            }
        }
        
        /// <summary>
        /// Update confidence display
        /// </summary>
        /// <param name="confidence">Confidence value (0-1)</param>
        private void UpdateConfidence(float confidence)
        {
            if (confidenceText == null) return;
            
            string confidenceStr = $"Confidence: {confidence:P0}";
            Color confidenceColor = confidence >= confidenceThreshold ? 
                highConfidenceColor : lowConfidenceColor;
            
            confidenceText.text = confidenceStr;
            confidenceText.color = confidenceColor;
        }
        
        /// <summary>
        /// Update stack list display
        /// </summary>
        /// <param name="stacks">List of chip stacks</param>
        private void UpdateStackList(System.Collections.Generic.List<ChipStack> stacks)
        {
            if (stackListParent == null || stackItemPrefab == null) return;
            
            // Clear existing items
            foreach (Transform child in stackListParent)
            {
                Destroy(child.gameObject);
            }
            
            // Create new items
            foreach (var stack in stacks)
            {
                CreateStackItem(stack);
            }
        }
        
        /// <summary>
        /// Create stack item UI
        /// </summary>
        /// <param name="stack">Chip stack data</param>
        private void CreateStackItem(ChipStack stack)
        {
            GameObject item = Instantiate(stackItemPrefab, stackListParent);
            
            // Find text components
            var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
            var valueText = item.GetComponentsInChildren<TextMeshProUGUI>()[1];
            var confidenceText = item.GetComponentsInChildren<TextMeshProUGUI>()[2];
            
            if (nameText != null)
            {
                nameText.text = stack.denomination?.displayName ?? "Unknown";
            }
            
            if (valueText != null)
            {
                valueText.text = $"${stack.value:F0}";
            }
            
            if (confidenceText != null)
            {
                confidenceText.text = $"{stack.confidence:P0}";
                confidenceText.color = stack.confidence >= confidenceThreshold ? 
                    highConfidenceColor : lowConfidenceColor;
            }
            
            // Set chip count
            var countText = item.GetComponentInChildren<TextMeshProUGUI>();
            if (countText != null)
            {
                countText.text = $"{stack.chipCount} chips";
            }
        }
        
        /// <summary>
        /// Show error message
        /// </summary>
        /// <param name="error">Error message</param>
        public void ShowError(string error)
        {
            if (totalValueText != null)
            {
                totalValueText.text = "Error";
                totalValueText.color = lowConfidenceColor;
            }
            
            if (confidenceText != null)
            {
                confidenceText.text = error;
                confidenceText.color = lowConfidenceColor;
            }
            
            ShowPanel(true);
        }
        
        /// <summary>
        /// Show or hide panel
        /// </summary>
        /// <param name="show">Whether to show panel</param>
        public void ShowPanel(bool show)
        {
            if (resultsCanvas != null)
            {
                resultsCanvas.gameObject.SetActive(show);
            }
        }
        
        /// <summary>
        /// Request recalculation
        /// </summary>
        private void RequestRecalculate()
        {
            OnRecalculateRequested?.Invoke();
            
            if (enableDebugLogs)
                Debug.Log("[ResultsPanel] Recalculation requested");
        }
        
        /// <summary>
        /// Request export
        /// </summary>
        private void RequestExport()
        {
            OnExportRequested?.Invoke();
            
            if (enableDebugLogs)
                Debug.Log("[ResultsPanel] Export requested");
        }
        
        /// <summary>
        /// Clear all results
        /// </summary>
        public void ClearResults()
        {
            if (totalValueText != null)
                totalValueText.text = "";
                
            if (totalBBText != null)
                totalBBText.text = "";
                
            if (confidenceText != null)
                confidenceText.text = "";
            
            // Clear stack list
            if (stackListParent != null)
            {
                foreach (Transform child in stackListParent)
                {
                    Destroy(child.gameObject);
                }
            }
            
            ShowPanel(false);
        }
    }
}
