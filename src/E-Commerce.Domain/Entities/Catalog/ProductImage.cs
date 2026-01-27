using E_Commerce.Domain.Entities.Abstract;

namespace E_Commerce.Domain.Entities.Catalog
{
    public class ProductImage : BaseEntity
    {
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsMain { get; set; } = false;

        // Navigation
        public Product? Product { get; set; }
    }
}
