using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PokerChipAnalyzer.AR;
using UnityEngine.XR.ARFoundation;

namespace PokerChipAnalyzer.UI
{
    /// <summary>
    /// Main HUD controller for scan guidance and progress
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Image progressRing;
        [SerializeField] private Button startScanButton;
        [SerializeField] private Button stopScanButton;
        [SerializeField] private Button settingsButton;
        
        [Header("Scan Guidance")]
        [SerializeField] private GameObject scanPathGuide;
        [SerializeField] private LineRenderer scanPathRenderer;
        [SerializeField] private float scanPathRadius = 0.3f;
        [SerializeField] private int scanPathPoints = 16;
        
        [Header("Settings")]
        [SerializeField] private float scanDuration = 8f; // seconds
        [SerializeField] private bool showScanPath = true;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Events
        public System.Action OnScanStarted;
        public System.Action OnScanStopped;
        public System.Action OnSettingsRequested;
        
        // Properties
        public bool IsScanning { get; private set; }
        public float ScanProgress { get; private set; }
        
        private ARSessionController arController;
        private float scanStartTime;
        
        private void Start()
        {
            InitializeHUD();
        }
        
        private void Update()
        {
            UpdateScanProgress();
            UpdateStatus();
        }
        
        /// <summary>
        /// Initialize HUD components
        /// </summary>
        private void InitializeHUD()
        {
            // Find AR controller
            arController = FindFirstObjectByType<ARSessionController>();
            
            // Setup buttons
            if (startScanButton != null)
                startScanButton.onClick.AddListener(StartScan);
                
            if (stopScanButton != null)
                stopScanButton.onClick.AddListener(StopScan);
                
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);
            
            // Initialize scan path
            InitializeScanPath();
            
            // Update initial state
            UpdateScanButtons();
            UpdateStatus();
            
            if (enableDebugLogs)
                Debug.Log("[HUDController] Initialized");
        }
        
        /// <summary>
        /// Initialize scan path visualization
        /// </summary>
        private void InitializeScanPath()
        {
            if (scanPathRenderer == null || !showScanPath) return;
            
            scanPathRenderer.positionCount = scanPathPoints + 1;
            scanPathRenderer.loop = true;
            scanPathRenderer.material = new Material(Shader.Find("Sprites/Default"));
            scanPathRenderer.material.color = Color.green;
            scanPathRenderer.startWidth = 0.01f;
            scanPathRenderer.endWidth = 0.01f;
            
            // Create circular path
            for (int i = 0; i <= scanPathPoints; i++)
            {
                float angle = (float)i / scanPathPoints * 2f * Mathf.PI;
                Vector3 point = new Vector3(
                    Mathf.Cos(angle) * scanPathRadius,
                    0.1f, // Slightly above table
                    Mathf.Sin(angle) * scanPathRadius
                );
                scanPathRenderer.SetPosition(i, point);
            }
            
            scanPathGuide.SetActive(false);
        }
        
        /// <summary>
        /// Start scanning process
        /// </summary>
        public void StartScan()
        {
            if (IsScanning) return;
            
            IsScanning = true;
            scanStartTime = Time.time;
            ScanProgress = 0f;
            
            // Show scan path
            if (scanPathGuide != null)
                scanPathGuide.SetActive(true);
            
            UpdateScanButtons();
            UpdateInstructionText();
            
            OnScanStarted?.Invoke();
            
            if (enableDebugLogs)
                Debug.Log("[HUDController] Scan started");
        }
        
        /// <summary>
        /// Stop scanning process
        /// </summary>
        public void StopScan()
        {
            if (!IsScanning) return;
            
            IsScanning = false;
            ScanProgress = 0f;
            
            // Hide scan path
            if (scanPathGuide != null)
                scanPathGuide.SetActive(false);
            
            UpdateScanButtons();
            UpdateInstructionText();
            
            OnScanStopped?.Invoke();
            
            if (enableDebugLogs)
                Debug.Log("[HUDController] Scan stopped");
        }
        
        /// <summary>
        /// Update scan progress
        /// </summary>
        private void UpdateScanProgress()
        {
            if (!IsScanning) return;
            
            float elapsed = Time.time - scanStartTime;
            ScanProgress = Mathf.Clamp01(elapsed / scanDuration);
            
            // Update progress ring
            if (progressRing != null)
            {
                progressRing.fillAmount = ScanProgress;
            }
            
            // Auto-stop when complete
            if (ScanProgress >= 1f)
            {
                StopScan();
            }
        }
        
        /// <summary>
        /// Update status text
        /// </summary>
        private void UpdateStatus()
        {
            if (statusText == null) return;

            if (arController == null)
            {
                statusText.text = "AR Not Available";
                return;
            }

            var sessionState = arController.GetSessionState();
            string status = "";

            // Reference ARSessionState.cs: None, Unsupported, CheckingAvailability, NeedsInstall, Installing, Ready, SessionInitializing, SessionTracking

            switch (sessionState)
            {
                case ARSessionState.None:
                    status = "Initializing AR...";
                    break;
                case ARSessionState.Unsupported:
                    status = "AR not supported on this device";
                    break;
                case ARSessionState.CheckingAvailability:
                    status = "Checking AR availability...";
                    break;
                case ARSessionState.NeedsInstall:
                    status = "AR software needs to be installed";
                    break;
                case ARSessionState.Installing:
                    status = "Installing AR software...";
                    break;
                case ARSessionState.Ready:
                    status = "AR Ready";
                    break;
                case ARSessionState.SessionInitializing:
                    status = "AR session initializing...";
                    break;
                case ARSessionState.SessionTracking:
                    status = IsScanning ? "Scanning..." : "Ready to Scan";
                    break;
                default:
                    status = "Unknown State";
                    break;
            }

            statusText.text = status;
        }
        
        /// <summary>
        /// Update instruction text
        /// </summary>
        private void UpdateInstructionText()
        {
            if (instructionText == null) return;
            
            string instruction = "";
            
            if (IsScanning)
            {
                instruction = "Move device in circular motion around chips";
            }
            else if (arController != null && arController.IsSessionRunning)
            {
                instruction = "Position device 20-30cm above table, then tap 'Start Scan'";
            }
            else
            {
                instruction = "Waiting for AR to initialize...";
            }
            
            instructionText.text = instruction;
        }
        
        /// <summary>
        /// Update scan button states
        /// </summary>
        private void UpdateScanButtons()
        {
            if (startScanButton != null)
                startScanButton.interactable = !IsScanning && arController != null && arController.IsSessionRunning;
                
            if (stopScanButton != null)
                stopScanButton.interactable = IsScanning;
        }
        
        /// <summary>
        /// Open settings panel
        /// </summary>
        private void OpenSettings()
        {
            OnSettingsRequested?.Invoke();
            
            if (enableDebugLogs)
                Debug.Log("[HUDController] Settings requested");
        }
        
        /// <summary>
        /// Show scan guidance
        /// </summary>
        /// <param name="show">Whether to show guidance</param>
        public void ShowScanGuidance(bool show)
        {
            if (scanPathGuide != null)
                scanPathGuide.SetActive(show && showScanPath);
        }
        
        /// <summary>
        /// Set scan duration
        /// </summary>
        /// <param name="duration">Duration in seconds</param>
        public void SetScanDuration(float duration)
        {
            scanDuration = Mathf.Clamp(duration, 1f, 30f);
        }
    }
}
