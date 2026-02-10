using System;

namespace E_Commerce.Domain.DomainEvents.Ordering.Order
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