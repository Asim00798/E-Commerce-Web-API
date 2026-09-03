using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Finance.IntegrationEvents;

public sealed class PaymentCompletedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string? ProviderTransactionId { get; }

    public PaymentCompletedIntegrationEvent(
        Guid paymentId,
        Guid orderId,
        decimal amount,
        string currency,
        string? providerTransactionId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        Currency = currency;
        ProviderTransactionId = providerTransactionId;
    }
}