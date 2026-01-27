using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class ProductAttribute : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Guid CategoryAttributeId { get; set; }
        public string Value { get; set; } = string.Empty;

        // Navigation
        public Product? Product { get; set; }
        public CategoryAttribute? CategoryAttribute { get; set; }
    }
}
