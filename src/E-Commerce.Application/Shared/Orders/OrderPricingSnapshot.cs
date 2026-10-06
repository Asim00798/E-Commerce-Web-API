namespace E_Commerce.Application.Shared.Orders;

/// <summary>
/// Authoritative order pricing snapshot. Finance uses this instead of
/// accepting Amount, Currency, and CustomerId from the client.
/// </summary>
public sealed record OrderPricingSnapshot(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency);