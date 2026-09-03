namespace E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

public sealed class ProductVariantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Sku { get; init; }
    public decimal PriceAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
}