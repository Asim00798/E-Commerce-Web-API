using System;

namespace E_Commerce.Domain.BoundedContexts.CoreCommerce.Ordering.Ordering.Order
{
    public sealed class OrderPaid : DomainEvent
    {
        public Guid AggregateId { get; }

        public OrderPaid(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}