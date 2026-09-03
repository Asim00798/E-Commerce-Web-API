namespace E_Commerce.Api.DTOs.Catalog.Products.Responses;

public sealed class ProductListResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public Guid BrandId { get; set; }
    public Guid CategoryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal MinPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int TotalStock { get; set; }
}
