using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PokerChipAnalyzer.Detection;
using PokerChipAnalyzer.Domain;
using System.Collections.Generic;

namespace PokerChipAnalyzer.Tests
{
    /// <summary>
    /// Manager for testing with poker chip photos in Unity Editor
    /// </summary>
    public class PhotoTestManager : MonoBehaviour
    {
        [Header("Photo Testing")]
        [SerializeField] private RawImage photoDisplay;
        [SerializeField] private Texture2D[] testPhotos;
        [SerializeField] private int currentPhotoIndex = 0;
        
        [Header("Test Configuration")]
        [SerializeField] private bool enablePhotoMode = true;
        [SerializeField] private float simulatedChipThickness = 3.3f;
        [SerializeField] private int[] expectedChipCounts; // Expected chip counts for each photo
        
        [Header("UI Components")]
        [SerializeField] private Button nextPhotoButton;
        [SerializeField] private Button prevPhotoButton;
        [SerializeField] private TextMeshProUGUI photoInfoText;
        [SerializeField] private TextMeshProUGUI testResultsText;
        
        [Header("Components")]
        [SerializeField] private StackRegionSelector regionSelector;
        [SerializeField] private StackHeightEstimator heightEstimator;
        [SerializeField] private CalculationService calculationService;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Test data
        private List<TestResult> testResults = new List<TestResult>();
        
        private struct TestResult
        {
            public int photoIndex;
            public int expectedChips;
            public int calculatedChips;
            public float accuracy;
            public bool isCorrect;
        }
        
        private void Start()
        {
            InitializePhotoTest();
        }
        
        /// <summary>
        /// Initialize photo testing system
        /// </summary>
        private void InitializePhotoTest()
        {
            if (!enablePhotoMode) return;
            
            // Setup buttons
            if (nextPhotoButton != null)
                nextPhotoButton.onClick.AddListener(NextPhoto);
                
            if (prevPhotoButton != null)
                prevPhotoButton.onClick.AddListener(PreviousPhoto);
            
            // Setup region selector events
            if (regionSelector != null)
            {
                regionSelector.OnRegionAdded += OnRegionSelected;
            }
            
            // Initialize arrays if needed
            if (expectedChipCounts == null || expectedChipCounts.Length == 0)
            {
                expectedChipCounts = new int[testPhotos.Length];
                for (int i = 0; i < expectedChipCounts.Length; i++)
                {
                    expectedChipCounts[i] = 10; // Default to 10 chips
                }
            }
            
            // Show first photo
            ShowCurrentPhoto();
            
            if (enableDebugLogs)
                Debug.Log("[PhotoTestManager] Photo testing initialized");
        }
        
        /// <summary>
        /// Show current photo
        /// </summary>
        private void ShowCurrentPhoto()
        {
            if (testPhotos == null || testPhotos.Length == 0)
            {
                Debug.LogWarning("[PhotoTestManager] No test photos available");
                return;
            }
            
            if (currentPhotoIndex >= 0 && currentPhotoIndex < testPhotos.Length)
            {
                if (photoDisplay != null)
                {
                    photoDisplay.texture = testPhotos[currentPhotoIndex];
                }
                
                UpdatePhotoInfo();
            }
        }
        
        /// <summary>
        /// Update photo information display
        /// </summary>
        private void UpdatePhotoInfo()
        {
            if (photoInfoText == null) return;
            
            int expectedChips = (currentPhotoIndex < expectedChipCounts.Length) 
                ? expectedChipCounts[currentPhotoIndex] 
                : 0;
                
            photoInfoText.text = $"Photo {currentPhotoIndex + 1}/{testPhotos.Length}\n" +
                                $"Expected: {expectedChips} chips\n" +
                                $"Thickness: {simulatedChipThickness}mm";
        }
        
        /// <summary>
        /// Handle region selection for testing
        /// </summary>
        /// <param name="region">Selected region</param>
        private void OnRegionSelected(Rect region)
        {
            if (!enablePhotoMode) return;
            
            // Simulate height estimation with photo data
            SimulateHeightEstimation(region);
        }
        
        /// <summary>
        /// Simulate height estimation for photo testing
        /// </summary>
        /// <param name="region">Selected region</param>
        private void SimulateHeightEstimation(Rect region)
        {
            // Generate simulated depth data based on photo analysis
            float simulatedHeight = GenerateSimulatedHeight(region);
            int calculatedChips = Mathf.RoundToInt(simulatedHeight / simulatedChipThickness);
            
            // Calculate confidence based on region size and position
            float confidence = CalculatePhotoConfidence(region);
            
            // Record test result
            int expectedChips = (currentPhotoIndex < expectedChipCounts.Length) 
                ? expectedChipCounts[currentPhotoIndex] 
                : 0;
                
            var testResult = new TestResult
            {
                photoIndex = currentPhotoIndex,
                expectedChips = expectedChips,
                calculatedChips = calculatedChips,
                accuracy = expectedChips > 0 ? 1f - Mathf.Abs(expectedChips - calculatedChips) / (float)expectedChips : 0f,
                isCorrect = Mathf.Abs(expectedChips - calculatedChips) <= 1
            };
            
            testResults.Add(testResult);
            
            // Update display
            UpdateTestResults();
            
            if (enableDebugLogs)
            {
                Debug.Log($"[PhotoTestManager] Photo {currentPhotoIndex + 1}: " +
                         $"Expected {expectedChips}, Calculated {calculatedChips}, " +
                         $"Accuracy {testResult.accuracy:P1}");
            }
        }
        
        /// <summary>
        /// Generate simulated height based on photo region
        /// </summary>
        /// <param name="region">Selected region</param>
        /// <returns>Simulated height in mm</returns>
        private float GenerateSimulatedHeight(Rect region)
        {
            // Simple simulation: larger regions = more chips
            // This is a placeholder - in real implementation, you'd analyze the photo
            float regionArea = region.width * region.height;
            float baseHeight = 10f; // Base height in mm
            float areaMultiplier = regionArea * 50f; // Scale factor
            
            // Add some randomness to simulate real measurement variance
            float randomFactor = Random.Range(0.8f, 1.2f);
            
            return (baseHeight + areaMultiplier) * randomFactor;
        }
        
        /// <summary>
        /// Calculate confidence based on region properties
        /// </summary>
        /// <param name="region">Selected region</param>
        /// <returns>Confidence value (0-1)</returns>
        private float CalculatePhotoConfidence(Rect region)
        {
            // Confidence based on region size and position
            float sizeConfidence = Mathf.Clamp01(region.width * region.height * 4f); // Larger regions = higher confidence
            float positionConfidence = 0.8f; // Assume good position for photo testing
            
            return (sizeConfidence + positionConfidence) * 0.5f;
        }
        
        /// <summary>
        /// Update test results display
        /// </summary>
        private void UpdateTestResults()
        {
            if (testResultsText == null) return;
            
            if (testResults.Count == 0)
            {
                testResultsText.text = "No test results yet";
                return;
            }
            
            int correctTests = 0;
            float totalAccuracy = 0f;
            
            foreach (var result in testResults)
            {
                if (result.isCorrect) correctTests++;
                totalAccuracy += result.accuracy;
            }
            
            float averageAccuracy = totalAccuracy / testResults.Count;
            
            testResultsText.text = $"Tests: {testResults.Count}\n" +
                                 $"Correct: {correctTests}\n" +
                                 $"Accuracy: {averageAccuracy:P1}";
        }
        
        /// <summary>
        /// Go to next photo
        /// </summary>
        public void NextPhoto()
        {
            if (testPhotos == null || testPhotos.Length == 0) return;
            
            currentPhotoIndex = (currentPhotoIndex + 1) % testPhotos.Length;
            ShowCurrentPhoto();
            
            // Clear regions for new photo
            if (regionSelector != null)
            {
                regionSelector.ClearAllRegions();
            }
        }
        
        /// <summary>
        /// Go to previous photo
        /// </summary>
        public void PreviousPhoto()
        {
            if (testPhotos == null || testPhotos.Length == 0) return;
            
            currentPhotoIndex = (currentPhotoIndex - 1 + testPhotos.Length) % testPhotos.Length;
            ShowCurrentPhoto();
            
            // Clear regions for new photo
            if (regionSelector != null)
            {
                regionSelector.ClearAllRegions();
            }
        }
        
        /// <summary>
        /// Load photos from Resources folder
        /// </summary>
        [ContextMenu("Load Photos from Resources")]
        public void LoadPhotosFromResources()
        {
            var photos = Resources.LoadAll<Texture2D>("TestPhotos");
            testPhotos = photos;
            
            if (enableDebugLogs)
                Debug.Log($"[PhotoTestManager] Loaded {photos.Length} photos from Resources/TestPhotos");
        }
        
        /// <summary>
        /// Export test results
        /// </summary>
        [ContextMenu("Export Test Results")]
        public void ExportTestResults()
        {
            if (testResults.Count == 0)
            {
                Debug.LogWarning("[PhotoTestManager] No test results to export");
                return;
            }
            
            string results = "Photo Test Results\n";
            results += "==================\n";
            
            foreach (var result in testResults)
            {
                results += $"Photo {result.photoIndex + 1}: " +
                          $"Expected {result.expectedChips}, " +
                          $"Calculated {result.calculatedChips}, " +
                          $"Accuracy {result.accuracy:P1}, " +
                          $"Correct: {result.isCorrect}\n";
            }
            
            Debug.Log(results);
        }
        
        /// <summary>
        /// Reset all test results
        /// </summary>
        [ContextMenu("Reset Test Results")]
        public void ResetTestResults()
        {
            testResults.Clear();
            UpdateTestResults();
            
            if (enableDebugLogs)
                Debug.Log("[PhotoTestManager] Test results reset");
        }
    }
}
