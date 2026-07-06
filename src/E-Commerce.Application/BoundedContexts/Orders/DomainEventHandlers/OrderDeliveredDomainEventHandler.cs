using Domain.SharedKernel.Events;
using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering.Ordering.Order;

namespace E_Commerce.Application.BoundedContexts.Orders.DomainEventHandlers;

/// <summary>
/// Creates an OrderDeliveredIntegrationEvent and writes it to the Outbox atomically
/// with the order state change.
/// </summary>
public class OrderDeliveredDomainEventHandler : IDomainEventHandler<OrderDelivered>
{
    private readonly IOutboxMessageWriter _outboxWriter;

    public OrderDeliveredDomainEventHandler(IOutboxMessageWriter outboxWriter)
    {
        _outboxWriter = outboxWriter;
    }

    public async Task Handle(OrderDelivered domainEvent, CancellationToken ct)
    {
        var integrationEvent = new OrderDeliveredIntegrationEvent(
            domainEvent.AggregateId);

        await _outboxWriter.WriteAsync(integrationEvent, ct);
    }
}