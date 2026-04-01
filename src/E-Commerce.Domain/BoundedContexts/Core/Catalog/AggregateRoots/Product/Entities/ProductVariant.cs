using E_Commerce.Domain.SharedKernel.Abstract;
using E_Commerce.Domain.SharedKernel.Interfaces;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Catalog.AggregateRoots.Product.Entities
{
    public class ProductVariant : BaseEntity, IEntity<ProductVariant>
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Red, Large"
        public string? SKU { get; set; }
        public Money Price { get; set; } = null!;
        public int StockQuantity { get; set; }
    }
}
