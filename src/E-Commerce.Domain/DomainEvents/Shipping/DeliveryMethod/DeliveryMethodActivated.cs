using System;

namespace E_Commerce.Domain.DomainEvents.Shipping.DeliveryMethod
{
    public sealed class DeliveryMethodActivated : DomainEvent
    {
        public Guid AggregateId { get; }

        public DeliveryMethodActivated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}