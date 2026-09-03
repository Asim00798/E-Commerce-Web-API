using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities;

public sealed class ProductImage : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid FileId { get; private set; }
    public string? AltText { get; private set; }
    public bool IsMain { get; internal set; } = false;

    private ProductImage()
    {
        // EF Core
    }

    internal ProductImage(Guid productId, Guid fileId, string? altText = null)
    {
        ProductId = productId;
        FileId = fileId;
        AltText = altText;
    }
}