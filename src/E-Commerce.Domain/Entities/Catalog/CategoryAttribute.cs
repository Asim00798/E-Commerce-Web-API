using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class CategoryAttribute : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public AttributeType Type { get; set; } = AttributeType.String;

        // Navigation
        public Category? Category { get; set; }
    }

    public enum AttributeType
    {
        String = 1,
        Number = 2,
        Boolean = 3,
        Date = 4
    }
}
