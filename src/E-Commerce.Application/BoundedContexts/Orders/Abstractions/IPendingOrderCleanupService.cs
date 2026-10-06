namespace E_Commerce.Application.BoundedContexts.Orders.Abstractions;

/// <summary>
/// Read-only query for pending orders that have exceeded the expiration threshold.
/// Does not mutate state or save changes. Cancellation and stock restoration
/// are performed by dedicated integration event handlers.
/// </summary>
public interface IPendingOrderCleanupService
{
    Task<IReadOnlyList<Guid>> GetExpiredPendingOrderIdsAsync(
        TimeSpan expirationThreshold,
        CancellationToken cancellationToken = default);
}