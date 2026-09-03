using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Shipping.IntegrationEvents;

public sealed class ShipmentDeliveredIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid ShipmentId { get; }
    public Guid OrderId { get; }
    public DateTime DeliveredAtUtc { get; }

    public ShipmentDeliveredIntegrationEvent(
        Guid shipmentId,
        Guid orderId,
        DateTime deliveredAtUtc)
    {
        ShipmentId = shipmentId;
        OrderId = orderId;
        DeliveredAtUtc = deliveredAtUtc;
    }
}