using System;

namespace E_Commerce.Domain.BoundedContexts.Ordering.Ordering.OrderItem
{
    public sealed class OrderItemReturned : DomainEvent
    {
        public Guid AggregateId { get; }

        public OrderItemReturned(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}