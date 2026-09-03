namespace E_Commerce.Application.BoundedContexts.Orders.Abstractions;

public interface IPendingOrderCleanupService
{
    /// <summary>
    /// Marks all pending orders older than the threshold as payment failed.
    /// Returns the IDs of the affected orders.
    /// This method does NOT save changes; the caller is responsible for saving.
    /// </summary>
    Task<IReadOnlyList<Guid>> ExpirePendingOrdersAsync(
        TimeSpan expirationThreshold,
        CancellationToken cancellationToken = default);
}