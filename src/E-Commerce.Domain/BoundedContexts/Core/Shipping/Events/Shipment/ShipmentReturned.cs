#if false
using System;

namespace E_Commerce.Domain.Events.Shipping.Shipment
{
    public sealed class ShipmentReturned : DomainEvent
    {
        public Guid AggregateId { get; }

        public ShipmentReturned(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif