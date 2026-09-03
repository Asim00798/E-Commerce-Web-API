using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;

public sealed class OrdersExpiredIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public IReadOnlyList<Guid> ExpiredOrderIds { get; }

    public OrdersExpiredIntegrationEvent(IReadOnlyList<Guid> expiredOrderIds
        , DateTime expiredAt)
    {
        ExpiredOrderIds = expiredOrderIds;
        OccurredAt = expiredAt;
    }
}