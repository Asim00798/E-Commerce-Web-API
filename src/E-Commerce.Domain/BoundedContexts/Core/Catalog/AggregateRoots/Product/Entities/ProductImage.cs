using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities
{
    public class ProductImage : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public Guid FileId { get; private set; }
        public string? AltText { get; private set; }
        public bool IsMain { get; internal set; } = false;

        public ProductImage(Guid productId, Guid fileId, string? altText = null)
        {
            ProductId = productId;
            FileId = fileId;
            AltText = altText;          
        }
    }
}
