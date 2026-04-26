#if false
using System;

namespace E_Commerce.Domain.Events.Shipping.Delivery
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
#endif