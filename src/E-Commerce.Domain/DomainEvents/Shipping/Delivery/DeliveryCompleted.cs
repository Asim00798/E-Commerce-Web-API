using System;

namespace E_Commerce.Domain.DomainEvents.Shipping.Delivery
{
    public sealed class DeliveryCompleted : DomainEvent
    {
        public Guid AggregateId { get; }

        public DeliveryCompleted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}