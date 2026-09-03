namespace E_Commerce.Api.DTOs.Catalog.Products.Requests;

public sealed class UpdateProductVariantPriceRequest
{
    public decimal NewPriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
