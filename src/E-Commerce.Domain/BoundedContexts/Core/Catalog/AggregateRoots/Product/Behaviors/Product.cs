using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors;

public sealed partial class Product : BaseEntity, IAggregateRoot
{
    private readonly List<ProductImage> _images = new();
    private readonly List<ProductVariant> _variants = new();
    private readonly List<string> _tags = new();

    public ProductDescription Description { get; private set; } = null!;
    public Guid BrandId { get; private set; }
    public Guid CategoryId { get; private set; }
    public ProductStatus Status { get; private set; }

    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    private Product()
    {
        // EF Core
    }

    private Product(
        ProductDescription description,
        Guid brandId,
        Guid categoryId,
        List<string>? tags = null)
    {
        Description = description;
        BrandId = brandId;
        CategoryId = categoryId;
        Status = ProductStatus.Draft;
        _tags = tags?.Distinct().ToList() ?? new List<string>();
    }

    public static Product Create(
        ProductDescription description,
        Guid brandId,
        Guid categoryId,
        List<string>? tags = null)
    {
        return new Product(description, brandId, categoryId, tags);
    }
}