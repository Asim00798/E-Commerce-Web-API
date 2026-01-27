using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Red, Large"
        public string? SKU { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        // Navigation
        public Product? Product { get; set; }
    }
}
