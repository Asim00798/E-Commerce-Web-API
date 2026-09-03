using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Events;

public sealed class ShipmentReturnedDomainEvent : DomainEvent
{
    public Guid ShipmentId { get; }
    public Guid OrderId { get; }
    public DateTime ReturnedAtUtc { get; }

    public ShipmentReturnedDomainEvent(
        Guid shipmentId,
        Guid orderId,
        DateTime returnedAtUtc)
    {
        ShipmentId = shipmentId;
        OrderId = orderId;
        ReturnedAtUtc = returnedAtUtc;
    }
}