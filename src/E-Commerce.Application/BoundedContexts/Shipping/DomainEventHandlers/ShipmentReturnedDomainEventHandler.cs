using Domain.SharedKernel.Events;
using E_Commerce.Application.BoundedContexts.Shipping.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Events;

namespace E_Commerce.Application.BoundedContexts.Shipping.DomainEventHandlers;

public sealed class ShipmentReturnedDomainEventHandler
    : IDomainEventHandler<ShipmentReturnedDomainEvent>
{
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IAppContext _appContext;

    public ShipmentReturnedDomainEventHandler(
        IOutboxMessageWriter outboxWriter,
        IAppContext appContext)
    {
        _outboxWriter = outboxWriter;
        _appContext = appContext;
    }

    public async Task Handle(
        ShipmentReturnedDomainEvent domainEvent,
        CancellationToken ct)
    {
        var integrationEvent = new ShipmentReturnedIntegrationEvent(
            domainEvent.ShipmentId,
            domainEvent.OrderId,
            domainEvent.ReturnedAtUtc)
        {
            CorrelationId = _appContext.CorrelationId
        };

        await _outboxWriter.WriteAsync(integrationEvent, ct);
    }
}