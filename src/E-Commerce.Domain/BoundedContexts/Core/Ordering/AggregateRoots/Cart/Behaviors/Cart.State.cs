using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Cart.Behaviors
{
    public partial class Cart
    {
        public void UpdateQuantity(Guid productVariantId, int newQuantity)
        {
            if (newQuantity <= 0)
                throw new CartException("Quantity must be positive.");

            var item = _items.FirstOrDefault(x => x.ProductVariantId == productVariantId)
                ?? throw new CartException("Cart item not found.");

            item.ChangeQuantity(newQuantity);
        }

        public void RemoveItem(Guid productVariantId)
        {
            var item = _items.FirstOrDefault(x => x.ProductVariantId == productVariantId);

            if (item is not null)
                _items.Remove(item);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
