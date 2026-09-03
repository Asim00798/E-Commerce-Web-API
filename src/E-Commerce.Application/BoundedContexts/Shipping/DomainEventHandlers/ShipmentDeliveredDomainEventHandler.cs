using Domain.SharedKernel.Events;
using E_Commerce.Application.BoundedContexts.Shipping.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Events;

namespace E_Commerce.Application.BoundedContexts.Shipping.DomainEventHandlers;

public sealed class ShipmentDeliveredDomainEventHandler
    : IDomainEventHandler<ShipmentDeliveredDomainEvent>
{
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IAppContext _appContext;

    public ShipmentDeliveredDomainEventHandler(
        IOutboxMessageWriter outboxWriter,
        IAppContext appContext)
    {
        _outboxWriter = outboxWriter;
        _appContext = appContext;
    }

    public async Task Handle(
        ShipmentDeliveredDomainEvent domainEvent,
        CancellationToken ct)
    {
        var integrationEvent = new ShipmentDeliveredIntegrationEvent(
            domainEvent.ShipmentId,
            domainEvent.OrderId,
            domainEvent.DeliveredAtUtc)
        {
            CorrelationId = _appContext.CorrelationId
        };

        await _outboxWriter.WriteAsync(integrationEvent, ct);
    }
}