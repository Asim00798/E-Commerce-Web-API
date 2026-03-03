using E_Commerce.Domain.SharedKernel.Abstract;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Product.Entities
{
    public class ProductImage : BaseEntity
    {
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsMain { get; set; } = false;

    }
}
