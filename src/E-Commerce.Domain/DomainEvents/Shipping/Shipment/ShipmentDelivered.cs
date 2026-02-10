using System;

namespace E_Commerce.Domain.DomainEvents.Shipping.Shipment
{
    public sealed class ShipmentDelivered : DomainEvent
    {
        public Guid AggregateId { get; }

        public ShipmentDelivered(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}