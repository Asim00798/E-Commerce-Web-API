using System;

namespace E_Commerce.Domain.DomainEvents.Ordering.OrderItem
{
    public sealed class OrderItemDamagedReported : DomainEvent
    {
        public Guid AggregateId { get; }

        public OrderItemDamagedReported(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}