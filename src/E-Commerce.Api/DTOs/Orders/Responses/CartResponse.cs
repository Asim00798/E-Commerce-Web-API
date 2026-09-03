namespace E_Commerce.Api.DTOs.Orders.Responses;

public sealed class CartResponse
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public IReadOnlyList<CartItemResponse> Items { get; init; } = new List<CartItemResponse>();
}