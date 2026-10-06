using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEventHandlers;

/// <summary>
/// Cancels pending orders whose expiration was reported by the
/// ExpirePendingOrdersJobHandler.
///
/// For each order:
///   - If already Cancelled, skip (idempotent within the batch).
///   - If not PendingPayment, skip (payment already completed, order advanced).
///   - Otherwise Order.Cancel() → raises OrderCancelledDomainEvent →
///     OrderCancelledIntegrationEvent written to the Outbox atomically.
///
/// Per-order transaction. A failure on one order does not roll back orders
/// already cancelled in the same batch.
///
/// Concurrency with user-initiated cancel is handled by RowVersion on Order:
/// if the two paths race, one wins, the other throws ConcurrencyException
/// and the Outbox marks the message Failed for retry. On retry, the order is
/// already Cancelled and is skipped.
/// </summary>
public sealed class CancelExpiredOrdersIntegrationEventHandler
    : IIntegrationEventHandler<OrdersExpiredIntegrationEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelExpiredOrdersIntegrationEventHandler> _logger;

    public CancelExpiredOrdersIntegrationEventHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<CancelExpiredOrdersIntegrationEventHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(
        OrdersExpiredIntegrationEvent integrationEvent,
        CancellationToken ct)
    {
        foreach (var orderId in integrationEvent.ExpiredOrderIds)
        {
            ct.ThrowIfCancellationRequested();
            await CancelOrderAsync(orderId, ct);
        }
    }

    private async Task CancelOrderAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order is null)
        {
            _logger.LogWarning(
                "Expired order {OrderId} not found; skipping cancellation.",
                orderId);
            return;
        }

        if (order.Status == OrderStatus.Cancelled)
            return; // already processed

        if (order.Status != OrderStatus.PendingPayment)
        {
            _logger.LogInformation(
                "Order {OrderId} is in status {Status}; skipping expiration cancellation.",
                orderId, order.Status);
            return;
        }

        order.Cancel();
        await _orderRepository.UpdateAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Cancelled expired order {OrderId}.", orderId);
    }
}