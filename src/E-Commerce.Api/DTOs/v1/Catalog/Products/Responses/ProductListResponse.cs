namespace E_Commerce.Api.DTOs.v1.Catalog.Products.Responses;

public sealed class ProductListResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public Guid BrandId { get; init; }
    public Guid CategoryId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal MinPrice { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int TotalStock { get; init; }
}
