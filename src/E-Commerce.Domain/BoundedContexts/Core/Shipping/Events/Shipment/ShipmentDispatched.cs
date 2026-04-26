#if false
using System;

namespace E_Commerce.Domain.Events.Shipping.Shipment
{
    public sealed class ShipmentDispatched : DomainEvent
    {
        public Guid AggregateId { get; }

        public ShipmentDispatched(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
#endif