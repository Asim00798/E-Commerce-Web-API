using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;

public class OrderDeliveredIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public Guid OrderId { get; }

    public OrderDeliveredIntegrationEvent(Guid orderId)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        OrderId = orderId;
    }
}