namespace E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public Guid CategoryId { get; init; }
    public Guid? BrandId { get; init; }
    public IReadOnlyCollection<ProductVariantDto> Variants { get; init; } = Array.Empty<ProductVariantDto>();
}
