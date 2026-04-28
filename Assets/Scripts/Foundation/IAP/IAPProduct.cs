using UnityEngine.Purchasing;

namespace XGame.Scripts.IAP
{
    public class IAPProduct
    {
        public string ProductId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Price { get; private set; }
        public ProductType ProductType { get; private set; }
        public Product OriginalProduct { get; private set; }
        public bool IsAvailable { get; private set; }
        public bool HasReceipt { get; private set; }

        public IAPProduct(Product product)
        {
            OriginalProduct = product;
            ProductId = product.definition.id;
            Title = product.metadata.localizedTitle;
            Description = product.metadata.localizedDescription;
            Price = product.metadata.localizedPriceString;
            ProductType = product.definition.type;
            IsAvailable = product.availableToPurchase;
            HasReceipt = !string.IsNullOrEmpty(product.receipt);
        }
    }
}