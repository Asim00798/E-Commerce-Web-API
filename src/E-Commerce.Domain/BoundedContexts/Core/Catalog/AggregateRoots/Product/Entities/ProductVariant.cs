using E_Commerce.Domain.SharedKernel.Abstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities
{
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public string Name { get; private set; } = string.Empty; // e.g., "Red, Large"
        public string? SKU { get; private set; }
        public Money Price { get; private set; } = null!;
        public int StockQuantity { get; internal set; } // Internal set for adjustment from aggregate

        // Added parameterless constructor for ORM if needed, otherwise just the formal one.
        private ProductVariant() { }

        public ProductVariant(Guid productId, string name, string? sku, Money price, int stockQuantity)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Variant name cannot be empty.", nameof(name));
            
            if (price == null || price.Amount <= 0)
                throw new ArgumentException("Price must be positive.", nameof(price));
                
            if (stockQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));

            ProductId = productId;
            Name = name;
            SKU = sku;
            Price = price;
            StockQuantity = stockQuantity;
        }

        public void UpdatePrice(Money newPrice)
        {
            if (newPrice == null || newPrice.Amount <= 0)
                throw new ArgumentException("Price must be positive.", nameof(newPrice));
                
            Price = newPrice;
        }
    }
}
