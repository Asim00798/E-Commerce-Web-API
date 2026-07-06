using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;

public class OrdersExpiredIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public List<Guid> ExpiredOrderIds { get; }
    public int ExpiredCount { get; }
    public DateTime ExpiredAt { get; }

    public OrdersExpiredIntegrationEvent(
        List<Guid> expiredOrderIds,
        int expiredCount,
        DateTime expiredAt)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        ExpiredOrderIds = expiredOrderIds;
        ExpiredCount = expiredCount;
        ExpiredAt = expiredAt;
    }
}