using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Finance.IntegrationEvents;

public sealed class RefundCompletedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
    public Guid OrderId { get; init; }
    public Guid RefundId { get; }
    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public string Currency { get; }

    public RefundCompletedIntegrationEvent(
        Guid refundId,
        Guid paymentId,
        Guid orderId,
        decimal amount,
        string currency)
    {
        RefundId = refundId;
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        Currency = currency;
    }
}