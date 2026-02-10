using System;

namespace E_Commerce.Domain.DomainEvents.Shipping.Shipment
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