using System;

namespace E_Commerce.Domain.BoundedContexts.Ordering.Ordering.Order
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