using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

namespace E_Commerce.Api.DTOs.Catalog.Products.Responses;

public sealed class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public Guid BrandId { get; set; }
    public Guid CategoryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
    public IReadOnlyList<ProductImageDto> Images { get; set; } = Array.Empty<ProductImageDto>();
    public IReadOnlyList<ProductVariantDto> Variants { get; set; } = Array.Empty<ProductVariantDto>();
}
