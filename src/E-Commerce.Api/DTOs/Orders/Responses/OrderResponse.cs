namespace E_Commerce.Api.DTOs.Orders.Responses;

public sealed class OrderResponse
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal ShippingFee { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime PlacedAtUtc { get; init; }
    public DateTime? CancelledAtUtc { get; init; }
    public DateTime? DeliveredAtUtc { get; init; }
    public DateTime? RefundedAtUtc { get; init; }
    public IReadOnlyList<OrderItemResponse> Items { get; init; } = new List<OrderItemResponse>();
}