#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering.Ordering.Order
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
#endif