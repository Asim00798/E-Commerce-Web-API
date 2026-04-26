#if false
using System;

namespace E_Commerce.Domain.Events.Shipping.ShipmentItem
{
    public sealed class ShipmentItemReturned : DomainEvent
    {
        public Guid AggregateId { get; }

        public ShipmentItemReturned(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif