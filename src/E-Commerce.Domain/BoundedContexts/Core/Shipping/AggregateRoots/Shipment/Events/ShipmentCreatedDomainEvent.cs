using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Events;

public sealed class ShipmentCreatedDomainEvent : DomainEvent
{
    public Guid ShipmentId { get; }
    public Guid OrderId { get; }

    public ShipmentCreatedDomainEvent(Guid shipmentId, Guid orderId)
    {
        ShipmentId = shipmentId;
        OrderId = orderId;
    }
}