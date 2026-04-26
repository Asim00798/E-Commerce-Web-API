#if false
using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering.Ordering.OrderItem
{
    public sealed class OrderItemSubstituted : DomainEvent
    {
        public Guid AggregateId { get; }

        public OrderItemSubstituted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif