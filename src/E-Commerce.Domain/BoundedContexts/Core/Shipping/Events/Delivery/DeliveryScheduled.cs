using System;

namespace E_Commerce.Domain.Events.Shipping.Delivery
{
    public sealed class DeliveryScheduled : DomainEvent
    {
        public Guid AggregateId { get; }

        public DeliveryScheduled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}