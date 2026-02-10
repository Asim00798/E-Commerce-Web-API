using System;

namespace E_Commerce.Domain.DomainEvents.Ordering.OrderItem
{
    public sealed class OrderItemBackordered : DomainEvent
    {
        public Guid AggregateId { get; }

        public OrderItemBackordered(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}