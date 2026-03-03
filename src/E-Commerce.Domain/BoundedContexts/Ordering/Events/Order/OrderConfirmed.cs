using System;

namespace E_Commerce.Domain.BoundedContexts.Ordering.Ordering.Order
{
    public sealed class OrderConfirmed : DomainEvent
    {
        public Guid AggregateId { get; }

        public OrderConfirmed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}