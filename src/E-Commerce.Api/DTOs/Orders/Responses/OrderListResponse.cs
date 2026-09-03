namespace E_Commerce.Api.DTOs.Orders.Responses;

public sealed class OrderListResponse
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime PlacedAtUtc { get; init; }
}