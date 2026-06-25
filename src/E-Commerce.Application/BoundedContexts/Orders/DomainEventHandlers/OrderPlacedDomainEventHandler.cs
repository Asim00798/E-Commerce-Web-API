using Domain.Orders.Events;
using Domain.SharedKernel.Events;
using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.DomainEventHandlers
{
    public class OrderPlacedDomainEventHandler : IDomainEventHandler<OrderPlacedDomainEvent>
    {
        private readonly IOutboxMessageWriter _outboxWriter;

        public OrderPlacedDomainEventHandler(IOutboxMessageWriter outboxWriter)
        {
            _outboxWriter = outboxWriter;
        }

        public async Task Handle(OrderPlacedDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            var integrationEvent = new OrderPlacedIntegrationEvent(
                domainEvent.OrderId,
                domainEvent.CustomerId,
                domainEvent.TotalAmount);

            await _outboxWriter.WriteAsync(integrationEvent, cancellationToken);
        }
    }
}
