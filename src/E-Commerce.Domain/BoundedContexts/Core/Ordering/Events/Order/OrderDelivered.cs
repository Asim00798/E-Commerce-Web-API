#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering.Ordering.Order
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
#endif