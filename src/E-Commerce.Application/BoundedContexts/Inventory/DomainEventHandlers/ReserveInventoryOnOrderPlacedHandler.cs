using Domain.Orders.Events;
using Domain.SharedKernel.Events;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Inventory.DomainEventHandlers;

/// <summary>
/// Synchronous handler that reserves inventory when an order is placed.
/// Executes in the same UnitOfWork transaction – failure rolls back the order.
/// </summary>
public class ReserveInventoryOnOrderPlacedHandler : IDomainEventHandler<OrderPlacedDomainEvent>
{
    private readonly ILogger<ReserveInventoryOnOrderPlacedHandler> _logger;

    public ReserveInventoryOnOrderPlacedHandler(ILogger<ReserveInventoryOnOrderPlacedHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderPlacedDomainEvent domainEvent, CancellationToken ct)
    {
        // TODO: Call real inventory service (e.g., _inventoryService.Reserve(...))
        _logger.LogInformation(
            "Reserving inventory for order {OrderId} (customer {CustomerId}, total {Total})",
            domainEvent.OrderId, domainEvent.CustomerId, domainEvent.TotalAmount);
        await Task.CompletedTask;
    }
}