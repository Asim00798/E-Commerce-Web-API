namespace E_Commerce.Api.DTOs.Orders.Requests;

/// <summary>
/// Request to add an item to the user's cart.
/// </summary>
public sealed class AddCartItemRequest
{
    public Guid ProductId { get; init; }
    public Guid ProductVariantId { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string VariantName { get; init; } = string.Empty;
    public decimal UnitPriceAmount { get; init; }
    public string UnitPriceCurrency { get; init; } = string.Empty;
    public int Quantity { get; init; }
}