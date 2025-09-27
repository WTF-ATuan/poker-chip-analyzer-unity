using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using System.Collections.Generic;

namespace PokerChipAnalyzer.IAP
{
    /// <summary>
    /// Manages in-app purchases and trial system
    /// </summary>
    public class IAPManager : MonoBehaviour, IStoreListener
    {
        [Header("IAP Settings")]
        [SerializeField] private string productId = "poker_chip_analyzer_full";
        [SerializeField] private int trialCount = 3;
        [SerializeField] private string trialCountKey = "trial_count";
        [SerializeField] private string purchaseKey = "purchase_complete";
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // Events
        public System.Action OnPurchaseSuccess;
        public System.Action<string> OnPurchaseFailedEvent;
        public System.Action OnTrialUsed;
        
        // Properties
        public bool IsPurchased => PlayerPrefs.GetInt(purchaseKey, 0) == 1;
        public int RemainingTrials => Mathf.Max(0, trialCount - GetUsedTrials());
        public bool HasTrialsLeft => RemainingTrials > 0;
        
        private IStoreController storeController;
        private IExtensionProvider extensionProvider;
        
        private void Start()
        {
            InitializeIAP();
        }
        
        /// <summary>
        /// Initialize IAP system
        /// </summary>
        private void InitializeIAP()
        {
            if (IsPurchased)
            {
                if (enableDebugLogs)
                    Debug.Log("[IAPManager] Already purchased");
                return;
            }
            
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(productId, ProductType.NonConsumable);
            
            UnityPurchasing.Initialize(this, builder);
            
            if (enableDebugLogs)
                Debug.Log("[IAPManager] IAP initialized");
        }
        
        /// <summary>
        /// Use a trial
        /// </summary>
        /// <returns>True if trial was used successfully</returns>
        public bool UseTrial()
        {
            if (IsPurchased)
            {
                return true; // Already purchased
            }
            
            if (!HasTrialsLeft)
            {
                return false; // No trials left
            }
            
            int usedTrials = GetUsedTrials();
            PlayerPrefs.SetInt(trialCountKey, usedTrials + 1);
            PlayerPrefs.Save();
            
            OnTrialUsed?.Invoke();
            
            if (enableDebugLogs)
                Debug.Log($"[IAPManager] Trial used. Remaining: {RemainingTrials}");
            
            return true;
        }
        
        /// <summary>
        /// Get number of used trials
        /// </summary>
        /// <returns>Number of used trials</returns>
        private int GetUsedTrials()
        {
            return PlayerPrefs.GetInt(trialCountKey, 0);
        }
        
        /// <summary>
        /// Purchase the full version
        /// </summary>
        public void PurchaseFullVersion()
        {
            if (IsPurchased)
            {
                OnPurchaseSuccess?.Invoke();
                return;
            }
            
            if (storeController == null)
            {
                OnPurchaseFailedEvent?.Invoke("Store not initialized");
                return;
            }
            
            Product product = storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                storeController.InitiatePurchase(product);
            }
            else
            {
                OnPurchaseFailedEvent?.Invoke("Product not available");
            }
        }
        
        /// <summary>
        /// Restore purchases (iOS)
        /// </summary>
        public void RestorePurchases()
        {
            if (extensionProvider != null)
            {
                extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions((success, error) =>
                {
                    if (enableDebugLogs)
                        Debug.Log($"[IAPManager] Restore result: Success={success}, Error={error}");
                });
            }
        }
        
        // IStoreListener implementation
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            storeController = controller;
            extensionProvider = extensions;
            
            if (enableDebugLogs)
                Debug.Log("[IAPManager] Store initialized");
        }
        
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            if (enableDebugLogs)
                Debug.LogError($"[IAPManager] Initialize failed: {error}");
        }
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            if (enableDebugLogs)
                Debug.LogError($"[IAPManager] Initialize failed: {error}, Message: {message}");
        }
        
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (args.purchasedProduct.definition.id == productId)
            {
                // Validate purchase
                if (ValidatePurchase(args.purchasedProduct))
                {
                    // Mark as purchased
                    PlayerPrefs.SetInt(purchaseKey, 1);
                    PlayerPrefs.Save();
                    
                    OnPurchaseSuccess?.Invoke();
                    
                    if (enableDebugLogs)
                        Debug.Log("[IAPManager] Purchase successful");
                }
                else
                {
                    OnPurchaseFailedEvent?.Invoke("Purchase validation failed");
                }
            }
            
            return PurchaseProcessingResult.Complete;
        }
        
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchaseFailedEvent?.Invoke(failureReason.ToString());
            
            if (enableDebugLogs)
                Debug.LogError($"[IAPManager] Purchase failed: {failureReason}");
        }
        
        /// <summary>
        /// Validate purchase receipt
        /// </summary>
        /// <param name="product">Purchased product</param>
        /// <returns>True if valid</returns>
        private bool ValidatePurchase(Product product)
        {
            // In production, validate with Apple/Google servers
            // For MVP, we'll do basic validation
            
            if (string.IsNullOrEmpty(product.receipt))
            {
                return false;
            }
            
            // Basic receipt validation
            try
            {
                var receipt = (Dictionary<string, object>)MiniJson.JsonDecode(product.receipt);
                return receipt.ContainsKey("Store") && receipt.ContainsKey("TransactionID");
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Reset trial count (for testing)
        /// </summary>
        [ContextMenu("Reset Trials")]
        public void ResetTrials()
        {
            PlayerPrefs.DeleteKey(trialCountKey);
            PlayerPrefs.DeleteKey(purchaseKey);
            PlayerPrefs.Save();
            
            if (enableDebugLogs)
                Debug.Log("[IAPManager] Trials reset");
        }
    }
}
