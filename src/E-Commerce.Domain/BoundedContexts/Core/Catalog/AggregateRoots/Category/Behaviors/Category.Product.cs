
namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Category.Behaviors
{
    public partial class Category
    {
        public void RegisterProduct(Guid productId)
        {
            if (_productIds.Contains(productId)) return;
            _productIds.Add(productId);
        }

        public void RemoveProduct(Guid productId)
        {
            _productIds.Remove(productId);
        }
    }
}
