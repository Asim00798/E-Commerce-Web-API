namespace E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

public record ProductVariantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Sku { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public int StockQuantity { get; init; }
}
