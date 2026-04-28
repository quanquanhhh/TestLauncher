using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Networking;
using System.Collections;
using Foundation;
using Foundation.Statistics.Facebook;
using Foundation.Statistics.FirebaseMgr;
using XGame.Scripts.IAP;
using Event = Foundation.Event;

namespace XGame.Scripts.IAP
{
    [System.Serializable]
    public class ProductPriceInfo
    {
        public string productId;
        public float price;
        public string currency;
        public string localizedPrice;
    }

    public class IAPManager : SingletonComponent<IAPManager>, IDetailedStoreListener, IIAPCallback
    {
        private IStoreController storeController;
        private IExtensionProvider extensionProvider;
        private IIAPCallback callback;
        private bool isInitialized = false;
        private bool isInitializing = false; // 防止重复初始化
        private Dictionary<string, IAPProduct> availableProducts = new Dictionary<string, IAPProduct>();
        private Dictionary<string, float> productPrices = new Dictionary<string, float>();
        private Dictionary<string, string> productCurrencies = new Dictionary<string, string>();
        private Dictionary<string, string> productLocalizedPrices = new Dictionary<string, string>();
        private List<ProductPriceInfo> allProductPrices = new List<ProductPriceInfo>();
        public bool IsInitialized { get { return isInitialized; } }

        private List<string> configProductIds = new();
        private Dictionary<string, (int, int)> configProductTypeAndPrices;

        // 临时保存当前购买的回调
        private Action<Product> currentPurchaseSuccessCallback;
        private Action<Product, PurchaseFailureReason, string> currentPurchaseFailedCallback;

        public async void InitProduct(List<string> productids, Dictionary<string, (int, int)> infos)
        {
            try
            {
                isInitialized = false;
                configProductIds = productids;
                configProductTypeAndPrices = infos;

                Initialize(this);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in InitProduct: {e.Message}");
            }
        }

        public int getPriceConfig(string id)
        {
            return configProductTypeAndPrices[id].Item2;
        }
        public void Initialize(IIAPCallback callback)
        {
            try
            {
                this.callback = callback;

                if (IsInitialized)
                {
                    callback?.OnInitializeSuccess();
                    return;
                }

                if (isInitializing)
                {
                    Debug.Log("IAP initialization already in progress");
                    return;
                }

                ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

                if (configProductIds != null && configProductIds.Count > 0)
                {
                    for (int i = 0; i < configProductIds.Count; i++)
                    {
                        try
                        {
                            var productId = configProductIds[i];
                            var type = (ProductType)configProductTypeAndPrices[productId].Item1;
                            builder.AddProduct(productId, type);
                            Debug.Log($"Added product: {productId} as {type}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error adding product {configProductIds[i]}: {e.Message}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No product IDs provided for IAP initialization or productType list is null/doesn't match");
                }

                isInitializing = true;
                TryInitialize(builder);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in Initialize: {e.Message}");
                isInitializing = false;
                callback?.OnInitializeFailed(InitializationFailureReason.PurchasingUnavailable, e.Message);
            }
        }

        private void TryInitialize(ConfigurationBuilder builder)
        {
            try
            {
                Debug.Log("Attempting IAP initialization");
                UnityPurchasing.Initialize(this, builder);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during IAP initialization: {e.Message}");
                isInitializing = false;
                try
                {
                    callback?.OnInitializeFailed(InitializationFailureReason.PurchasingUnavailable, e.Message);
                }
                catch (Exception callbackEx)
                {
                    Debug.LogError($"Error calling OnInitializeFailed: {callbackEx.Message}");
                }
            }
        }

        public void FetchProducts()
        {
            try
            {
                if (!IsInitialized)
                {
                    Debug.LogError("IAPManager not initialized");
                    if (callback != null && callback != this)
                    {
                        callback.OnProductsFetchedFailed("IAPManager not initialized");
                    }
                    return;
                }

                if (configProductIds == null || configProductIds.Count == 0)
                {
                    Debug.LogError("Product IDs list is empty");
                    if (callback != null && callback != this)
                    {
                        callback.OnProductsFetchedFailed("Product IDs list is empty");
                    }
                    return;
                }

                if (storeController == null)
                {
                    Debug.LogError("Store controller is null");
                    if (callback != null && callback != this)
                    {
                        callback.OnProductsFetchedFailed("Store controller is null");
                    }
                    return;
                }

                List<Product> products = new List<Product>();
                availableProducts.Clear();
                productPrices.Clear();
                productCurrencies.Clear();
                productLocalizedPrices.Clear();
                allProductPrices.Clear();

                foreach (var productId in configProductIds)
                {
                    try
                    {
                        Product product = storeController.products.WithID(productId);
                        if (product != null)
                        {
                            if (product.definition != null)
                            {
                                products.Add(product);
                                availableProducts[productId] = new IAPProduct(product);

                                float price = 0;
                                string currency = "USD";
                                string localizedPrice = "--";

                                if (product.metadata != null)
                                {
                                    price = (float)product.metadata.localizedPrice;
                                    currency = product.metadata.isoCurrencyCode;
                                    localizedPrice = product.metadata.localizedPriceString;

#if UNITY_EDITOR
                                    price = configProductTypeAndPrices[productId].Item2 / 100f;
                                    currency = "USD";
                                    localizedPrice = "$" + price;
#endif
                                }

                                productPrices[productId] = price;
                                productCurrencies[productId] = currency;
                                productLocalizedPrices[productId] = localizedPrice;

                                allProductPrices.Add(new ProductPriceInfo
                                {
                                    productId = productId,
                                    price = price,
                                    currency = currency,
                                    localizedPrice = localizedPrice
                                });

                                Debug.Log(
                                    "[IAP] Product Loaded\n" +
                                    $"id = {product.definition.id}\n" +
                                    $"storeSpecificId = {product.definition.storeSpecificId}\n" +
                                    $"availableToPurchase = {product.availableToPurchase}\n" +
                                    $"hasReceipt = {product.hasReceipt}\n" +
                                    $"localizedPrice = {localizedPrice}\n" +
                                    $"isoCurrencyCode = {currency}"
                                );
                            }
                            else
                            {
                                Debug.LogWarning($"Product {productId} has no definition");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Product {productId} not found in store controller");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error processing product {productId}: {e.Message}");
                    }
                }

                if (callback != null && callback != this)
                {
                    callback.OnProductsFetchedSuccess(products);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to fetch products: {e.Message}");
                if (callback != null && callback != this)
                {
                    callback.OnProductsFetchedFailed(e.Message);
                }
            }
        }

        public string GetLocalizedPrice(string productId)
        {
            if (productLocalizedPrices.TryGetValue(productId, out var price))
            {
                return price;
            }
            return "--";
        }

        private void ClearCurrentPurchaseCallbacks()
        {
            currentPurchaseSuccessCallback = null;
            currentPurchaseFailedCallback = null;
        }

        private void HandlePurchaseFailed(Product product, PurchaseFailureReason failureReason, string message, string source)
        {
            string finalMessage = string.IsNullOrWhiteSpace(message)
                ? $"Purchase failed: {failureReason}"
                : message;

            Debug.LogError(
                "[IAP] Purchase Failed\n" +
                $"source = {source}\n" +
                $"product.definition.id = {product?.definition?.id}\n" +
                $"product.availableToPurchase = {product?.availableToPurchase}\n" +
                $"product.hasReceipt = {product?.hasReceipt}\n" +
                $"product.transactionID = {product?.transactionID}\n" +
                $"product.storeSpecificId = {product?.definition?.storeSpecificId}\n" +
                $"localizedPrice = {product?.metadata?.localizedPriceString}\n" +
                $"isoCurrencyCode = {product?.metadata?.isoCurrencyCode}\n" +
                $"failureReason = {failureReason}\n" +
                $"message = {finalMessage}"
            );

            UIModule.Instance.Close<UIMaskLoading>();

            callback?.OnPurchaseFailed(product, failureReason, finalMessage);

            var failedCallback = currentPurchaseFailedCallback;
            ClearCurrentPurchaseCallbacks();
            failedCallback?.Invoke(product, failureReason, finalMessage);
        }

        public void PurchaseProduct(string productId, Action<Product> successCallback = null, Action<Product, PurchaseFailureReason, string> failedCallback = null)
        {
            currentPurchaseSuccessCallback = successCallback;
            currentPurchaseFailedCallback = failedCallback;

            if (!IsInitialized)
            {
                Debug.LogError("IAPManager not initialized");
                HandlePurchaseFailed(null, PurchaseFailureReason.PurchasingUnavailable, "IAPManager not initialized", "PurchaseProduct/NotInitialized");
                return;
            }

            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogError("Product ID is empty");
                HandlePurchaseFailed(null, PurchaseFailureReason.ProductUnavailable, "Product ID is empty", "PurchaseProduct/EmptyProductId");
                return;
            }

            Product originalProduct = storeController.products.WithID(productId);
            if (originalProduct == null)
            {
                Debug.LogError($"Product {productId} not found in store controller");
                HandlePurchaseFailed(null, PurchaseFailureReason.ProductUnavailable, $"Product {productId} not found in store controller", "PurchaseProduct/ProductNotFound");
                return;
            }

            Debug.Log(
                "[IAP] Start Buy\n" +
                $"product.definition.id = {originalProduct?.definition?.id}\n" +
                $"product.availableToPurchase = {originalProduct?.availableToPurchase}\n" +
                $"product.hasReceipt = {originalProduct?.hasReceipt}\n" +
                $"product.storeSpecificId = {originalProduct?.definition?.storeSpecificId}\n" +
                $"localizedPrice = {originalProduct?.metadata?.localizedPriceString}\n" +
                $"isoCurrencyCode = {originalProduct?.metadata?.isoCurrencyCode}"
            );

            if (!originalProduct.availableToPurchase)
            {
                Debug.LogError($"Product {originalProduct.metadata?.localizedTitle ?? productId} is not available for purchase");
                HandlePurchaseFailed(originalProduct, PurchaseFailureReason.ProductUnavailable, "Product is not available for purchase", "PurchaseProduct/NotAvailableToPurchase");
                return;
            }

            UIModule.Instance.ShowAsync<UIMaskLoading>();

            try
            {
                storeController.InitiatePurchase(originalProduct);
            }
            catch (Exception e)
            {
                HandlePurchaseFailed(originalProduct, PurchaseFailureReason.Unknown, $"InitiatePurchase exception: {e.Message}", "PurchaseProduct/InitiatePurchaseException");
            }
        }

        private bool ClickToRestore = true;

        public async void RestorePurchases(bool bClick)
        {
            ClickToRestore = bClick;
#if UNITY_EDITOR
            // GameController.Instance.ClaimLifeTimeVip();
            // GameController.Instance.ClaimMonthlyVip();
            // GameController.Instance.ClaimRemoveAd();
            // if(ClickToRestore) UIController.Instance.ShowPurchase(PurchaseStatus.Complete);
            // return;
#endif
            if (ClickToRestore) 
                await UIModule.Instance.ShowAsync<UIMaskLoading>();

            if (!IsInitialized)
            {
                Debug.LogError("IAPManager not initialized");
                callback?.OnRestorePurchasesFailed("IAPManager not initialized");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
            {
                Debug.Log("Restoring purchases for Apple App Store");
                var appleExtensions = extensionProvider.GetExtension<IAppleExtensions>();
                appleExtensions.RestoreTransactions((result, message) =>
                {
                    if (result)
                    {
                        Debug.Log("Restore purchases succeeded");
                        List<Product> restoredPurchases = new List<Product>();
                        Product[] allProducts = storeController.products.all;

                        foreach (Product product in allProducts)
                        {
                            if (product.hasReceipt)
                            {
                                restoredPurchases.Add(product);
                            }
                        }
                        callback?.OnRestorePurchasesSuccess(restoredPurchases);
                    }
                    else
                    {
                        Debug.LogError("Restore purchases failed: " + message);
                        callback?.OnRestorePurchasesFailed(message);
                    }
                });
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                Debug.Log("Restore purchases for Google Play Store");
                var googleExtensions = extensionProvider.GetExtension<IGooglePlayStoreExtensions>();
                googleExtensions.RestoreTransactions((result, message) =>
                {
                    if (result)
                    {
                        Debug.Log("Restore purchases succeeded");
                        List<Product> restoredPurchases = new List<Product>();
                        Product[] allProducts = storeController.products.all;
                        foreach (Product product in allProducts)
                        {
                            if (product.hasReceipt)
                            {
                                restoredPurchases.Add(product);
                            }
                        }
                        ClaimRestore(restoredPurchases);
                        callback?.OnRestorePurchasesSuccess(restoredPurchases);
                    }
                    else
                    {
                        Debug.LogError("Restore purchases failed: " + message);
                        callback?.OnRestorePurchasesFailed(message);
                    }
                });
            }
            else
            {
                Debug.LogWarning("Restore purchases is not supported on this platform");
                callback?.OnRestorePurchasesFailed("Restore purchases is not supported on this platform");
            }
        }

        public static long GetSubscriptionExpire(Product product, string introJson = null)
        {
            if (product == null)
            {
                return -1;
            }

            if (product.definition == null)
            {
                return -1;
            }

            if (product.definition.type != ProductType.Subscription)
            {
                return -1;
            }

            if (!product.hasReceipt || string.IsNullOrEmpty(product.receipt))
            {
                return -1;
            }

            try
            {
                var manager = new SubscriptionManager(product, introJson);
                var info = manager.getSubscriptionInfo();

                if (info == null)
                {
                    return -1;
                }

                var expireDate = info.getExpireDate();
                var isExpired = info.isExpired() == Result.True;
                if (isExpired)
                {
                    return -1;
                }

                long expireSeconds = new DateTimeOffset(expireDate).ToUnixTimeSeconds();
                return expireSeconds;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private void ClaimRestore(List<Product> restoredPurchases)
        {
            foreach (var item in restoredPurchases)
            {
                long expire = -1;
                if (item.definition.type == ProductType.Subscription)
                {
                    expire = GetSubscriptionExpire(item);
                    if (expire > 0)
                    {
                        continue;
                    }
                }

                string sId = item.definition.id;
                Event.Instance.SendEvent(new RestoreBuff(sId, expire));
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("IAP system initialized successfully");
            storeController = controller;
            extensionProvider = extensions;
            isInitialized = true;
            isInitializing = false;
            FetchProducts();
            callback?.OnInitializeSuccess();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            OnInitializeFailed(error, error.ToString());
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            try
            {
                Debug.LogError($"IAP system initialization failed: {error}, {message}");
                isInitializing = false;

                if (callback != null && callback != this)
                {
                    callback.OnInitializeFailed(error, message);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in OnInitializeFailed: {e.Message}");
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log($"Processing purchase: {args.purchasedProduct.definition.id}");

            Product product = args.purchasedProduct;
            callback?.OnPurchaseSuccess(product);

            var successCallback = currentPurchaseSuccessCallback;
            ClearCurrentPurchaseCallbacks();
            successCallback?.Invoke(product);

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            HandlePurchaseFailed(product, failureReason, null, "LegacyOnPurchaseFailed");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            var reason = failureDescription != null ? failureDescription.reason : PurchaseFailureReason.Unknown;
            var message = failureDescription != null ? failureDescription.message : null;
            HandlePurchaseFailed(product, reason, message, "DetailedOnPurchaseFailed");
        }

        public void OnPurchaseDeferred(Product product)
        {
            Debug.Log($"Purchase deferred: {product.definition.id}");
            UIModule.Instance.Close<UIMaskLoading>();
            callback?.OnPurchaseDeferred(product);
            ClearCurrentPurchaseCallbacks();
        }

        public IAPProduct GetProduct(string productId)
        {
            if (availableProducts.TryGetValue(productId, out var product))
            {
                return product;
            }
            return null;
        }

        public Dictionary<string, IAPProduct> GetAllProducts()
        {
            return availableProducts;
        }

        private void OnDestroy()
        {
            // Unity IAP handles cleanup automatically
        }

        public void OnInitializeSuccess()
        {
            Debug.Log("内购服务初始化成功");
        }

        public void OnProductsFetchedSuccess(List<Product> products)
        {
        }

        public void OnProductsFetchedFailed(string message)
        {
            Debug.Log($"查询产品失败: {message}");
        }

        public void OnPurchaseSuccess(Product product)
        {
            Debug.Log($"购买成功: {product.definition.id}");
            UIModule.Instance.Close<UIMaskLoading>();
            Event.Instance.SendEvent(new ShowTips("PurchaseSuccess", true));
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason, string message)
        {
            Event.Instance.SendEvent(new ShowTips("OnPurchaseFailed", true));
        }

        public void OnRestorePurchasesSuccess(List<Product> restoredPurchases)
        {
            Debug.Log($"恢复购买完成，共找到 {restoredPurchases.Count} 个购买记录");
            foreach (var product in restoredPurchases)
            {
                Debug.Log($"找到购买记录: {product.definition.id}");
            }

            if (!ClickToRestore) return;
            UIModule.Instance.Close<UIMaskLoading>();
            Event.Instance.SendEvent(new ShowTips("OnRestorePurchasesSuccess", true));
        }

        public void OnRestorePurchasesFailed(string message)
        {
            Debug.Log($"恢复购买失败: {message}");
            if (!ClickToRestore) return;
            UIModule.Instance.Close<UIMaskLoading>();
            Event.Instance.SendEvent(new ShowTips("OnRestorePurchasesFailed", true));
        }

        public void Purchase(string sProduct, Action claimAction, Action<Product> productDeal, string adjustkey, int price, string name, string from)
        {
#if UNITY_EDITOR
            claimAction?.Invoke();
            return;
#endif
            double sPrice = price / 100.0;
            string sProductName = name;
            string sProductId = sProduct;
            string sAdjustKey = adjustkey;

            Action<Product> logAction = (product) =>
            {
                FacebookMgr.Instance.TrackPurchaseEvent(sPrice);
                AdjustManager.Instance.LogAdjustRevenue("ynhmvm",sPrice, product.metadata.isoCurrencyCode);
                TBAMgr.Instance.SendLogPurchase(sPrice, sProductId,from, product.metadata.isoCurrencyCode);
                FirebaseMgr.Instance.ReportRevenue(sProductId, sProductName, sPrice, product.metadata.isoCurrencyCode, Guid.NewGuid().ToString());
            };

            PurchaseProduct(sProduct, (product) =>
            {
                claimAction?.Invoke();
                logAction?.Invoke(product);
                productDeal?.Invoke(product);
            });
        }
    }
}