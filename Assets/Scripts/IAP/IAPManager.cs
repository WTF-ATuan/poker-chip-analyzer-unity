using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;
using System.Linq;

namespace PokerChipAnalyzer.IAP
{
    /// <summary>
    /// Manages in-app purchases and trial system
    /// </summary>
    public class IAPManager : MonoBehaviour
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
        
        private StoreController storeController;
        private bool isInitialized = false;
        
        private async void Start()
        {
            await InitializeIAP();
        }
        
        /// <summary>
        /// Initialize IAP system using Unity IAP v5 new API
        /// </summary>
        private async System.Threading.Tasks.Task InitializeIAP()
        {
            if (IsPurchased)
            {
                if (enableDebugLogs)
                    Debug.Log("[IAPManager] Already purchased");
                return;
            }
            
            if (isInitialized)
                return;
            
            try
            {
                storeController = UnityIAPServices.StoreController();
                
                storeController.OnPurchasePending += OnPurchasePending;
                storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
                
                await storeController.Connect();
                
                FetchProducts();
                
                isInitialized = true;
                
                if (enableDebugLogs)
                    Debug.Log("[IAPManager] IAP initialized successfully");
            }
            catch (System.Exception e)
            {
                if (enableDebugLogs)
                    Debug.LogError($"[IAPManager] IAP initialization failed: {e.Message}");
                    
                OnPurchaseFailedEvent?.Invoke($"Initialization failed: {e.Message}");
            }
        }
        
        /// <summary>
        /// Fetch products from store
        /// </summary>
        private void FetchProducts()
        {
            var productsToFetch = new List<ProductDefinition>
            {
                new(productId, ProductType.NonConsumable)
            };
            
            storeController.FetchProducts(productsToFetch);
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
        /// Purchase the full version using Unity IAP v5 new API
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
            
            var product = storeController.GetProducts().FirstOrDefault(p => p.definition.id == productId);
            if (product != null)
            {
                storeController.PurchaseProduct(product);
            }
            else
            {
                OnPurchaseFailedEvent?.Invoke("Product not available");
            }
        }
        
        /// <summary>
        /// Restore purchases using Unity IAP v5 new API
        /// </summary>
        public void RestorePurchases()
        {
            if (storeController != null)
            {
                storeController.RestoreTransactions(OnTransactionsRestored);
            }
        }
        
        /// <summary>
        /// Handle transaction restore result
        /// </summary>
        private void OnTransactionsRestored(bool success, string error)
        {
            if (enableDebugLogs)
                Debug.Log($"[IAPManager] Restore result: Success={success}, Error={error}");
        }
        
        /// <summary>
        /// Handle purchase pending event
        /// </summary>
        private void OnPurchasePending(PendingOrder order)
        {
            if (enableDebugLogs)
                Debug.Log($"[IAPManager] Purchase pending: {order.CartOrdered.Items().First().Product.definition.id}");
            
            // Validate purchase before confirming
            if (ValidatePurchase(order))
            {
                // Confirm the purchase
                storeController.ConfirmPurchase(order);
            }
            else
            {
                OnPurchaseFailedEvent?.Invoke("Purchase validation failed");
            }
        }
        
        /// <summary>
        /// Handle purchase confirmed event
        /// </summary>
        private void OnPurchaseConfirmed(Order order)
        {
            switch (order)
            {
                case FailedOrder failedOrder:
                    var failedProduct = failedOrder.CartOrdered.Items().First().Product.definition.id;
                    OnPurchaseFailedEvent?.Invoke($"Purchase failed: {failedOrder.FailureReason}");
                    
                    if (enableDebugLogs)
                        Debug.LogError($"[IAPManager] Purchase failed: {failedProduct}, {failedOrder.FailureReason}");
                    break;
                    
                case ConfirmedOrder confirmedOrder:
                    var productId = confirmedOrder.CartOrdered.Items().First().Product.definition.id;
                    
                    if (productId == this.productId)
                    {
                        // Mark as purchased
                        PlayerPrefs.SetInt(purchaseKey, 1);
                        PlayerPrefs.Save();
                        
                        OnPurchaseSuccess?.Invoke();
                        
                        if (enableDebugLogs)
                            Debug.Log($"[IAPManager] Purchase successful: {productId}");
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Validate purchase order
        /// </summary>
        /// <param name="order">Purchase order to validate</param>
        /// <returns>True if valid</returns>
        private bool ValidatePurchase(PendingOrder order)
        {
            // For MVP, we'll do basic validation
            // In production, validate with Apple/Google servers
            
            var product = order.CartOrdered.Items().First().Product;
            
            // Basic validation - check if product ID matches
            return product.definition.id == productId;
        }
        
        /// <summary>
        /// Check if product is available for purchase
        /// </summary>
        /// <returns>True if product is available</returns>
        public bool IsProductAvailable()
        {
            if (!isInitialized || storeController == null)
                return false;
                
            var product = storeController.GetProducts().FirstOrDefault(p => p.definition.id == productId);
            return product != null;
        }
        
        /// <summary>
        /// Get product information
        /// </summary>
        /// <returns>Product info or null if not available</returns>
        public Product GetProductInfo()
        {
            if (!isInitialized || storeController == null)
                return null;
                
            return storeController.GetProducts().FirstOrDefault(p => p.definition.id == productId);
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
