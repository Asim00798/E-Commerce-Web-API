#if false
using System;

namespace E_Commerce.Domain.Events.Shipping.Delivery
{
    public sealed class DeliveryCanceled : DomainEvent
    {
        public Guid AggregateId { get; }

        public DeliveryCanceled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif