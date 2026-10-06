using E_Commerce.Application.BoundedContexts.Orders.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;

namespace E_Commerce.Infrastructure.Persistence.Modules.Orders.Services;

/// <summary>
/// Query-only service that returns the IDs of pending orders older than
/// the configured expiration threshold.
///
/// This service does NOT mutate order state or restore stock. The two
/// concerns are handled separately by integration event handlers reacting
/// to OrdersExpiredIntegrationEvent:
///
///   - CancelExpiredOrdersIntegrationEventHandler
///   - RestoreStockOnExpiredOrdersIntegrationEventHandler
///
/// Keeping detection separate from mutation means the job has exactly one
/// responsibility (find candidates), and the mutation paths are idempotent
/// and independently retryable.
/// </summary>
public sealed class PendingOrderCleanupService : IPendingOrderCleanupService
{
    private readonly IOrderRepository _orderRepository;

    public PendingOrderCleanupService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<Guid>> GetExpiredPendingOrderIdsAsync(
        TimeSpan expirationThreshold,
        CancellationToken cancellationToken = default)
    {
        var expirationTime = DateTime.UtcNow - expirationThreshold;

        return await _orderRepository.GetPendingOrderIdsOlderThanAsync(
            expirationTime, cancellationToken);
    }
}