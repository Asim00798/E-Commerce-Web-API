using System;

namespace E_Commerce.Domain.DomainEvents.Ordering.Order
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