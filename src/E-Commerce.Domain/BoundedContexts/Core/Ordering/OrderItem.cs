using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoot.Product;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering
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
