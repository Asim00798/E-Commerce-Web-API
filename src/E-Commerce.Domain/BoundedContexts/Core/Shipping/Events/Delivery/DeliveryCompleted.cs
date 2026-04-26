#if false
using System;

namespace E_Commerce.Domain.Events.Shipping.Delivery
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
#endif