using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;

namespace E_Commerce.Domain.Entities.Ordering
{
    public class CartItem : BaseEntity
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public int Quantity { get; set; } = 1;

        // Navigation
        public Cart? Cart { get; set; }
        public Product? Product { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public decimal UnitPrice => ProductVariant?.Price ?? Product?.Price ?? 0;
        public decimal TotalPrice => UnitPrice * Quantity;

        public override void Validate()
        {
            base.Validate();

            if (Quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");
        }
    }
}
