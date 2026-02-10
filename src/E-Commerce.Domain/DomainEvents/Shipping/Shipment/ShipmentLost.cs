using System;

namespace E_Commerce.Domain.DomainEvents.Shipping.Shipment
{
    public sealed class ShipmentLost : DomainEvent
    {
        public Guid ShipmentLostId { get; }

        public ShipmentLost(Guid shipmentLostId)
        {
            ShipmentLostId = shipmentLostId;
        }
    }
}