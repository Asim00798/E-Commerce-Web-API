using System;

namespace E_Commerce.Domain.DomainEvents.Shipping.Shipment
{
    public sealed class ShipmentCanceled : DomainEvent
    {
        public Guid AggregateId { get; }

        public ShipmentCanceled(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}