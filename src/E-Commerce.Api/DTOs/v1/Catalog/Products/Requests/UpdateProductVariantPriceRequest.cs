namespace E_Commerce.Api.DTOs.v1.Catalog.Products.Requests;

public sealed class UpdateProductVariantPriceRequest
{
    public decimal NewPriceAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
}
