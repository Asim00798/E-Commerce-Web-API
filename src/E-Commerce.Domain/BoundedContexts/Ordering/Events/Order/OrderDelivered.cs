using System;

namespace E_Commerce.Domain.BoundedContexts.Ordering.Ordering.Order
{
    public sealed class OrderDelivered : DomainEvent
    {
        public Guid AggregateId { get; }

        public OrderDelivered(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}