using System;

namespace E_Commerce.Domain.DomainEvents.Ordering.Order
{
    public sealed class OrderShipped : DomainEvent
    {
        public Guid AggregateId { get; }

        public OrderShipped(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}