#if false
using System;

namespace E_Commerce.Domain.Events.Shipping.DeliveryMethod
{
    public sealed class DeliveryMethodCreated : DomainEvent
    {
        public Guid AggregateId { get; }

        public DeliveryMethodCreated(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif