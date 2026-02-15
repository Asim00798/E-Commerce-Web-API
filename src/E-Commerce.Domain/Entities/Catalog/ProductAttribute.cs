using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class ProductAttribute : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Guid CategoryAttributeId { get; set; }
        public string Value { get; private set; } = string.Empty;

        // Navigation
        public Product? Product { get; set; }
        public CategoryAttribute? CategoryAttribute { get; set; }

        // This ensures aggregate encapsulation and
        // prevents external code from mutating the entity directly.
        internal void UpdateValue(string newValue)
        {
            // Optionally validate type / format here
            Value = newValue;
        }
    }
}
