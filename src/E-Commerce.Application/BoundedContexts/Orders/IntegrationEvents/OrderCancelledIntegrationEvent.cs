using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;

public sealed class OrderCancelledIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public DateTime CancelledAtUtc { get; }

    public OrderCancelledIntegrationEvent(Guid orderId, Guid customerId, DateTime cancelledAtUtc)
    {
        OrderId = orderId;
        CustomerId = customerId;
        CancelledAtUtc = cancelledAtUtc;
    }
}