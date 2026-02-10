using System;

namespace E_Commerce.Domain.DomainEvents.Shipping.DeliveryMethod
{
    public sealed class DeliveryMethodDeactivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public DeliveryMethodDeactivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}