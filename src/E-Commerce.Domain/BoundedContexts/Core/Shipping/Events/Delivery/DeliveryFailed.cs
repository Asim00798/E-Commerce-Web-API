using System;

namespace E_Commerce.Domain.Events.Shipping.Delivery
{
    public sealed class DeliveryFailed : DomainEvent
    {
        public Guid AggregateId { get; }

        public DeliveryFailed(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}