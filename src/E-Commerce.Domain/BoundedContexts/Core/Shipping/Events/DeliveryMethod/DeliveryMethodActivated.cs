#if false
using System;

namespace E_Commerce.Domain.Events.Shipping.DeliveryMethod
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
#endif