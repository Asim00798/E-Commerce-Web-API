using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering.Ordering.Order
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