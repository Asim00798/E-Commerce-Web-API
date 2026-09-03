using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Events;

public sealed class ShipmentDeliveredDomainEvent : DomainEvent
{
    public Guid ShipmentId { get; }
    public Guid OrderId { get; }
    public DateTime DeliveredAtUtc { get; }

    public ShipmentDeliveredDomainEvent(
        Guid shipmentId,
        Guid orderId,
        DateTime deliveredAtUtc)
    {
        ShipmentId = shipmentId;
        OrderId = orderId;
        DeliveredAtUtc = deliveredAtUtc;
    }
}