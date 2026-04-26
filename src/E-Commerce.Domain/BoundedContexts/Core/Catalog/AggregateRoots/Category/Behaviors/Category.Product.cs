
namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors
{
    public partial class Category
    {
        public void AddProduct(Guid productId)
        {
            if (!_productIds.Contains(productId))
            {
                _productIds.Add(productId);
            }
        }

        public void RemoveProduct(Guid productId)
        {
            _productIds.Remove(productId);
        }
    }
}
