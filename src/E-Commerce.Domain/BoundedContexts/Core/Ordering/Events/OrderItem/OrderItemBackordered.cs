using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering.Ordering.OrderItem
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