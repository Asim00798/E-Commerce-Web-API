namespace E_Commerce.Api.DTOs.Catalog.Products.Requests;

public sealed class AddProductVariantRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}
