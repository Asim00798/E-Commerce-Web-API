using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

namespace E_Commerce.Api.DTOs.v1.Catalog.Products.Responses;

public sealed class ProductResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string? LongDescription { get; init; }
    public Guid BrandId { get; init; }
    public Guid CategoryId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<ProductImageDto> Images { get; init; } = Array.Empty<ProductImageDto>();
    public IReadOnlyList<ProductVariantDto> Variants { get; init; } = Array.Empty<ProductVariantDto>();
}
