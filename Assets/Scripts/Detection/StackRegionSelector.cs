using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PokerChipAnalyzer.Detection
{
    /// <summary>
    /// Allows user to manually select regions of interest for chip stacks
    /// </summary>
    public class StackRegionSelector : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private RectTransform selectionCanvas;
        [SerializeField] private Image selectionBoxPrefab;
        [SerializeField] private Button addRegionButton;
        [SerializeField] private Button clearAllButton;
        [SerializeField] private TextMeshProUGUI instructionText;
        
        [Header("Settings")]
        [SerializeField] private Color validRegionColor = Color.green;
        [SerializeField] private Color invalidRegionColor = Color.red;
        [SerializeField] private float minRegionSize = 0.1f; // 10% of screen
        [SerializeField] private float maxRegionSize = 0.8f; // 80% of screen
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Events
        public System.Action<Rect> OnRegionAdded;
        public System.Action<int> OnRegionRemoved;
        public System.Action OnAllRegionsCleared;
        
        // Properties
        public int RegionCount => regions.Count;
        public bool IsSelecting { get; private set; }
        
        private System.Collections.Generic.List<Rect> regions = new System.Collections.Generic.List<Rect>();
        private System.Collections.Generic.List<Image> regionBoxes = new System.Collections.Generic.List<Image>();
        private Vector2 selectionStart;
        private Image currentSelectionBox;
        
        private void Start()
        {
            InitializeSelector();
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        /// <summary>
        /// Initialize the region selector
        /// </summary>
        private void InitializeSelector()
        {
            if (addRegionButton != null)
                addRegionButton.onClick.AddListener(StartRegionSelection);
                
            if (clearAllButton != null)
                clearAllButton.onClick.AddListener(ClearAllRegions);
                
            UpdateInstructionText();
        }
        
        /// <summary>
        /// Handle user input for region selection (supports both touch and mouse)
        /// </summary>
        private void HandleInput()
        {
            if (!IsSelecting) return;
            
            // Handle touch input (mobile)
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        StartSelection(touch.position);
                        break;
                    case TouchPhase.Moved:
                        UpdateSelection(touch.position);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        FinishSelection(touch.position);
                        break;
                }
            }
            // Handle mouse input (editor/desktop)
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    StartSelection(Input.mousePosition);
                }
                else if (Input.GetMouseButton(0))
                {
                    UpdateSelection(Input.mousePosition);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    FinishSelection(Input.mousePosition);
                }
            }
        }
        
        /// <summary>
        /// Start region selection mode
        /// </summary>
        public void StartRegionSelection()
        {
            IsSelecting = true;
            UpdateInstructionText();
        }
        
        /// <summary>
        /// Start drawing a new selection
        /// </summary>
        /// <param name="inputPosition">Input position (touch or mouse)</param>
        private void StartSelection(Vector2 inputPosition)
        {
            Vector2 normalizedPos = new Vector2(
                inputPosition.x / Screen.width,
                inputPosition.y / Screen.height
            );
            
            selectionStart = normalizedPos;
            
            // Create selection box
            if (selectionBoxPrefab != null && selectionCanvas != null)
            {
                currentSelectionBox = Instantiate(selectionBoxPrefab, selectionCanvas);
                currentSelectionBox.color = validRegionColor;
            }
        }
        
        /// <summary>
        /// Update current selection box
        /// </summary>
        /// <param name="inputPosition">Input position (touch or mouse)</param>
        private void UpdateSelection(Vector2 inputPosition)
        {
            if (currentSelectionBox == null) return;
            
            Vector2 normalizedPos = new Vector2(
                inputPosition.x / Screen.width,
                inputPosition.y / Screen.height
            );
            
            // Calculate selection rectangle
            Vector2 min = Vector2.Min(selectionStart, normalizedPos);
            Vector2 max = Vector2.Max(selectionStart, normalizedPos);
            Rect selectionRect = new Rect(min, max - min);
            
            // Update selection box visual
            RectTransform rectTransform = currentSelectionBox.rectTransform;
            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Validate region size
            bool isValid = IsValidRegion(selectionRect);
            currentSelectionBox.color = isValid ? validRegionColor : invalidRegionColor;
        }
        
        /// <summary>
        /// Finish current selection
        /// </summary>
        /// <param name="inputPosition">Input position (touch or mouse)</param>
        private void FinishSelection(Vector2 inputPosition)
        {
            if (currentSelectionBox == null) return;
            
            Vector2 normalizedPos = new Vector2(
                inputPosition.x / Screen.width,
                inputPosition.y / Screen.height
            );
            
            // Calculate final selection rectangle
            Vector2 min = Vector2.Min(selectionStart, normalizedPos);
            Vector2 max = Vector2.Max(selectionStart, normalizedPos);
            Rect selectionRect = new Rect(min, max - min);
            
            if (IsValidRegion(selectionRect))
            {
                AddRegion(selectionRect);
                currentSelectionBox.color = validRegionColor;
                regionBoxes.Add(currentSelectionBox);
            }
            else
            {
                Destroy(currentSelectionBox.gameObject);
            }
            
            currentSelectionBox = null;
            IsSelecting = false;
            UpdateInstructionText();
        }
        
        /// <summary>
        /// Add a new region
        /// </summary>
        /// <param name="region">Normalized region coordinates (0-1)</param>
        public void AddRegion(Rect region)
        {
            regions.Add(region);
            OnRegionAdded?.Invoke(region);
            
            if (enableDebugLogs)
                Debug.Log($"[StackRegionSelector] Added region: {region}");
        }
        
        /// <summary>
        /// Remove region by index
        /// </summary>
        /// <param name="index">Region index</param>
        public void RemoveRegion(int index)
        {
            if (index >= 0 && index < regions.Count)
            {
                regions.RemoveAt(index);
                
                if (index < regionBoxes.Count)
                {
                    Destroy(regionBoxes[index].gameObject);
                    regionBoxes.RemoveAt(index);
                }
                
                OnRegionRemoved?.Invoke(index);
                
                if (enableDebugLogs)
                    Debug.Log($"[StackRegionSelector] Removed region at index {index}");
            }
        }
        
        /// <summary>
        /// Clear all regions
        /// </summary>
        public void ClearAllRegions()
        {
            regions.Clear();
            
            foreach (var box in regionBoxes)
            {
                if (box != null)
                    Destroy(box.gameObject);
            }
            regionBoxes.Clear();
            
            OnAllRegionsCleared?.Invoke();
            
            if (enableDebugLogs)
                Debug.Log("[StackRegionSelector] Cleared all regions");
        }
        
        /// <summary>
        /// Check if region is valid
        /// </summary>
        /// <param name="region">Region to validate</param>
        /// <returns>True if valid</returns>
        private bool IsValidRegion(Rect region)
        {
            return region.width >= minRegionSize && 
                   region.height >= minRegionSize &&
                   region.width <= maxRegionSize && 
                   region.height <= maxRegionSize;
        }
        
        /// <summary>
        /// Get region by index
        /// </summary>
        /// <param name="index">Region index</param>
        /// <returns>Region coordinates, or empty rect if invalid</returns>
        public Rect GetRegion(int index)
        {
            if (index >= 0 && index < regions.Count)
                return regions[index];
            return new Rect();
        }
        
        /// <summary>
        /// Get all regions
        /// </summary>
        /// <returns>Array of all regions</returns>
        public Rect[] GetAllRegions()
        {
            return regions.ToArray();
        }
        
        /// <summary>
        /// Update instruction text based on current state
        /// </summary>
        private void UpdateInstructionText()
        {
            if (instructionText == null) return;
            
            if (IsSelecting)
            {
                instructionText.text = "Drag to select chip stack region";
            }
            else
            {
                instructionText.text = $"Regions: {regions.Count} | Click 'Add Region' to start";
            }
        }
    }
}
