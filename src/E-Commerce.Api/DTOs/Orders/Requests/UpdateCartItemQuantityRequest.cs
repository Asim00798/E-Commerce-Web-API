namespace E_Commerce.Api.DTOs.Orders.Requests;

/// <summary>
/// Request to update the quantity of an item in the cart.
/// </summary>
public sealed class UpdateCartItemQuantityRequest
{
    public Guid ProductVariantId { get; init; }
    public int NewQuantity { get; init; }
}