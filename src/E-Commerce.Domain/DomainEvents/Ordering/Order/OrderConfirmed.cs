using System;

namespace E_Commerce.Domain.DomainEvents.Ordering.Order
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