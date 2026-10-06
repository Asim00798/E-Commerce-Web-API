namespace E_Commerce.Application.Shared.Orders;

/// <summary>
/// Shared contract for Finance to obtain authoritative order pricing.
/// Implemented by Ordering. Used to prevent client-tampered payment fields.
/// </summary>
public interface IOrderPricingReader
{
    Task<OrderPricingSnapshot?> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken ct = default);
}