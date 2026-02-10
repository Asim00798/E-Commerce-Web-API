using System;

namespace E_Commerce.Domain.DomainEvents.Shipping.Delivery
{
    public sealed class DeliveryStarted : DomainEvent
    {
        public Guid AggregateId { get; }

        public DeliveryStarted(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}