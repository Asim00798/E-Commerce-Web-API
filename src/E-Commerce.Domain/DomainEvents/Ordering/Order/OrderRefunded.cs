using System;

namespace E_Commerce.Domain.DomainEvents.Ordering.Order
{
    public sealed class OrderRefunded : DomainEvent
    {
        public Guid AggregateId { get; }

        public OrderRefunded(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}