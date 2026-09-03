using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Finance.IntegrationEvents;

public sealed class RefundFailedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid RefundId { get; }
    public Guid PaymentId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string? Reason { get; }

    public RefundFailedIntegrationEvent(
        Guid refundId,
        Guid paymentId,
        decimal amount,
        string currency,
        string? reason)
    {
        RefundId = refundId;
        PaymentId = paymentId;
        Amount = amount;
        Currency = currency;
        Reason = reason;
    }
}