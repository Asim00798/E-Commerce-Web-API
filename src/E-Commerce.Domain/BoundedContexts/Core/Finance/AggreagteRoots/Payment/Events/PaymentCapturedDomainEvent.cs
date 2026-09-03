using E_Commerce.Domain.SharedKernel.Events;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Events;

public sealed class PaymentCapturedDomainEvent : DomainEvent
{
    public PaymentCapturedDomainEvent(
        Guid paymentId,
        Guid orderId,
        Money amount,
        string? providerTransactionId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        ProviderTransactionId = providerTransactionId;
    }

    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public Money Amount { get; }
    public string? ProviderTransactionId { get; }
}