using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace XGame.Scripts.IAP
{
    public interface IIAPCallback
    {
        void OnInitializeSuccess();
        void OnInitializeFailed(InitializationFailureReason reason, string message);
        void OnProductsFetchedSuccess(List<Product> products);
        void OnProductsFetchedFailed(string message);
        void OnPurchaseSuccess(Product product);
        void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason, string message);
        void OnPurchaseDeferred(Product product);
        void OnRestorePurchasesSuccess(List<Product> restoredPurchases);
        void OnRestorePurchasesFailed(string message);
    }
}