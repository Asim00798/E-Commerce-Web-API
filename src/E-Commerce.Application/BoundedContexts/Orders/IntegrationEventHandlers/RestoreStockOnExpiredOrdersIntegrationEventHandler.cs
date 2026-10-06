using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Stock;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Enums;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEventHandlers;

/// <summary>
/// Restores stock for orders cancelled due to expiration.
///
/// Runs as a separate handler from CancelExpiredOrdersIntegrationEventHandler
/// so the two concerns are independently retryable.
///
/// Ordering contract: this handler must see the order in Cancelled status.
/// If the cancellation handler has not yet committed (because the two handlers
/// ran in reverse order within the same dispatch), this handler throws so the
/// Outbox retries. On retry, the cancellation is already committed and this
/// handler completes.
///
/// Per-order transaction. Stock increases for a single order commit together.
/// </summary>
public sealed class RestoreStockOnExpiredOrdersIntegrationEventHandler
    : IIntegrationEventHandler<OrdersExpiredIntegrationEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IStockService _stockService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RestoreStockOnExpiredOrdersIntegrationEventHandler> _logger;

    public RestoreStockOnExpiredOrdersIntegrationEventHandler(
        IOrderRepository orderRepository,
        IStockService stockService,
        IUnitOfWork unitOfWork,
        ILogger<RestoreStockOnExpiredOrdersIntegrationEventHandler> logger)
    {
        _orderRepository = orderRepository;
        _stockService = stockService;
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
            await RestoreStockAsync(orderId, ct);
        }
    }

    private async Task RestoreStockAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order is null)
        {
            _logger.LogWarning(
                "Expired order {OrderId} not found; skipping stock restoration.",
                orderId);
            return;
        }

        if (order.Status != OrderStatus.Cancelled)
        {
            // Cancellation handler hasn't run yet, or this order was cancelled
            // by a different path (user-initiated). Throw so the dispatcher
            // retries after the cancellation handler completes.
            throw new InvalidOperationException(
                $"Order {orderId} is in status {order.Status}; " +
                "stock restoration requires Cancelled status.");
        }

        foreach (var item in order.Items)
        {
            var result = await _stockService.IncreaseStockAsync(
                item.ProductId,
                item.ProductVariantId,
                item.Quantity,
                ct);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Stock restoration failed for order {order.Id}, " +
                    $"product {item.ProductId}: " +
                    $"{string.Join("; ", result.Errors)}");
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Restored stock for expired order {OrderId}.", orderId);
    }
}