using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Events;

public sealed class ShipmentShippedDomainEvent : DomainEvent
{
    public Guid ShipmentId { get; }
    public Guid OrderId { get; }
    public DateTime ShippedAtUtc { get; }

    public ShipmentShippedDomainEvent(
        Guid shipmentId,
        Guid orderId,
        DateTime shippedAtUtc)
    {
        ShipmentId = shipmentId;
        OrderId = orderId;
        ShippedAtUtc = shippedAtUtc;
    }
}