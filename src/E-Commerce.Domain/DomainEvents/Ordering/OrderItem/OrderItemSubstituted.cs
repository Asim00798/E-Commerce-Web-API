using System;

namespace E_Commerce.Domain.DomainEvents.Ordering.OrderItem
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