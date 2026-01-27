using E_Commerce.Domain.Entities.Abstract;
using E_Commerce.Domain.Entities.Catalog;

namespace E_Commerce.Domain.Entities.Ordering
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }

        // Navigation
        public Order? Order { get; set; }
        public Product? Product { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public decimal TotalPrice => UnitPrice * Quantity;

        public override void Validate()
        {
            base.Validate();

            if (Quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            if (UnitPrice < 0)
                throw new InvalidOperationException("UnitPrice cannot be negative.");
        }
    }
}
