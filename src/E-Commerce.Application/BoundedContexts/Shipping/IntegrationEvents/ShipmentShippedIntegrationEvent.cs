using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Shipping.IntegrationEvents;

public sealed class ShipmentShippedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid ShipmentId { get; }
    public Guid OrderId { get; }
    public DateTime ShippedAtUtc { get; }

    public ShipmentShippedIntegrationEvent(
        Guid shipmentId,
        Guid orderId,
        DateTime shippedAtUtc)
    {
        ShipmentId = shipmentId;
        OrderId = orderId;
        ShippedAtUtc = shippedAtUtc;
    }
}