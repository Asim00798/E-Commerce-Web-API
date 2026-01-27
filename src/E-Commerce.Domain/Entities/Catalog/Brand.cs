using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class Brand : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }

        // Navigation
        public ICollection<Product>? Products { get; set; }
    }
}
