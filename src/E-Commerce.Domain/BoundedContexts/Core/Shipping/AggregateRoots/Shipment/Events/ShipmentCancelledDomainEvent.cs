using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Events;

public sealed class ShipmentCancelledDomainEvent : DomainEvent
{
    public Guid ShipmentId { get; }
    public Guid OrderId { get; }
    public DateTime CancelledAtUtc { get; }

    public ShipmentCancelledDomainEvent(
        Guid shipmentId,
        Guid orderId,
        DateTime cancelledAtUtc)
    {
        ShipmentId = shipmentId;
        OrderId = orderId;
        CancelledAtUtc = cancelledAtUtc;
    }
}