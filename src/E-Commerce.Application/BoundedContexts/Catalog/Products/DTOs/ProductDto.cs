namespace E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

public sealed class ProductDto
{
    public Guid Id { get; init; }
    public ProductDescriptionDto Description { get; init; } = null!;
    public Guid BrandId { get; init; }
    public Guid CategoryId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ProductImageDto> Images { get; init; } = Array.Empty<ProductImageDto>();
    public IReadOnlyList<ProductVariantDto> Variants { get; init; } = Array.Empty<ProductVariantDto>();
}