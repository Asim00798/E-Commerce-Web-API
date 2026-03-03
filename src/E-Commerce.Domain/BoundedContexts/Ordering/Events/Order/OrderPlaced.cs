using System;

namespace E_Commerce.Domain.BoundedContexts.Ordering.Ordering.Order
{
    public sealed class OrderPlaced : DomainEvent
    {
        public Guid AggregateId { get; }

        public OrderPlaced(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}