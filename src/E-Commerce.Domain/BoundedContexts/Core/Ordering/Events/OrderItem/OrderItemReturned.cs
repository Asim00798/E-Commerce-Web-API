using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering.Ordering.OrderItem
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