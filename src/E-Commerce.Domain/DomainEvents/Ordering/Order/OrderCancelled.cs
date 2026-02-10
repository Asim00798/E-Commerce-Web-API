using System;

namespace E_Commerce.Domain.DomainEvents.Ordering.Order
{
    public sealed class OrderCancelled : DomainEvent
    {
        public Guid AggregateId { get; }

        public OrderCancelled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}