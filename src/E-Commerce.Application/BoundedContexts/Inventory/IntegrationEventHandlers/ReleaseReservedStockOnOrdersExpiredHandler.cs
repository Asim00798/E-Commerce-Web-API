using E_Commerce.Application.BoundedContexts.Inventory.Abstractions;
using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Inventory.IntegrationEventHandlers;

public class ReleaseReservedStockOnOrdersExpiredHandler
    : IIntegrationEventHandler<OrdersExpiredIntegrationEvent>
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<ReleaseReservedStockOnOrdersExpiredHandler> _logger;

    public ReleaseReservedStockOnOrdersExpiredHandler(
        IInventoryService inventoryService,
        ILogger<ReleaseReservedStockOnOrdersExpiredHandler> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async Task HandleAsync(
        OrdersExpiredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Releasing reserved stock for {Count} expired orders",
            integrationEvent.ExpiredCount);

        foreach (var orderId in integrationEvent.ExpiredOrderIds)
        {
            await _inventoryService.ReleaseReservedStockAsync(orderId, cancellationToken);
        }
    }
}