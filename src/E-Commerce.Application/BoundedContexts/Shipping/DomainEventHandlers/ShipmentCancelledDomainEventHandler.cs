using Domain.SharedKernel.Events;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Events;
using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Application.BoundedContexts.Shipping.DomainEventHandlers;

public sealed class ShipmentCancelledDomainEventHandler
    : IDomainEventHandler<ShipmentCancelledDomainEvent>
{
    public Task Handle(
        ShipmentCancelledDomainEvent domainEvent,
        CancellationToken ct)
    {
        // No outbound integration event is currently required for cancellation.
        return Task.CompletedTask;
    }
}