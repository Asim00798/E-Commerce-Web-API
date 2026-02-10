using System;

namespace E_Commerce.Domain.DomainEvents.Shipping.Shipment
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