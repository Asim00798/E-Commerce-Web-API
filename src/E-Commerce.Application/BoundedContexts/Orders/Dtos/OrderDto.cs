using Application.BoundedContexts.Ordering.Dtos;

namespace E_Commerce.Application.BoundedContexts.Orders.Dtos;

public sealed class OrderDto
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
    public IReadOnlyList<OrderItemDto> Items { get; init; } = new List<OrderItemDto>();
}