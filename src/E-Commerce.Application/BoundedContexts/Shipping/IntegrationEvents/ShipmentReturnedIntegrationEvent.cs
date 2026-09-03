using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Shipping.IntegrationEvents;

public sealed class ShipmentReturnedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid ShipmentId { get; }
    public Guid OrderId { get; }
    public DateTime ReturnedAtUtc { get; }

    public ShipmentReturnedIntegrationEvent(
        Guid shipmentId,
        Guid orderId,
        DateTime returnedAtUtc)
    {
        ShipmentId = shipmentId;
        OrderId = orderId;
        ReturnedAtUtc = returnedAtUtc;
    }
}