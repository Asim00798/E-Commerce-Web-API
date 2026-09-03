using Domain.SharedKernel.Events;
using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Shared.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Events;

namespace E_Commerce.Application.BoundedContexts.Orders.DomainEventHandlers;

public sealed class OrderCancelledDomainEventHandler
    : IDomainEventHandler<OrderCancelledDomainEvent>
{
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IAppContext _appContext;

    public OrderCancelledDomainEventHandler(
        IOutboxMessageWriter outboxWriter,
        IAppContext appContext)
    {
        _outboxWriter = outboxWriter;
        _appContext = appContext;
    }

    public async Task Handle(OrderCancelledDomainEvent domainEvent, CancellationToken ct)
    {
        var integrationEvent = new OrderCancelledIntegrationEvent(
            orderId: domainEvent.OrderId,
            customerId: domainEvent.CustomerId,
            cancelledAtUtc: DateTime.UtcNow)
        {
            CorrelationId = _appContext.CorrelationId
        };

        await _outboxWriter.WriteAsync(integrationEvent, ct);
    }
}