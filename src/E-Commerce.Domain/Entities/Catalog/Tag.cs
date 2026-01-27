using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        // Navigation
        public ICollection<Product>? Products { get; set; }
    }
}
