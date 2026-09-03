using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Finance.IntegrationEvents;

public sealed class PaymentFailedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid PaymentId { get; }
    public Guid OrderId { get; }

    public PaymentFailedIntegrationEvent(Guid paymentId, Guid orderId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
    }
}