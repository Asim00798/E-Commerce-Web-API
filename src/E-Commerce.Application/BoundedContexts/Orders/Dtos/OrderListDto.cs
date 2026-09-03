namespace E_Commerce.Application.BoundedContexts.Orders.Dtos;

public sealed class OrderListDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime PlacedAtUtc { get; init; }
}