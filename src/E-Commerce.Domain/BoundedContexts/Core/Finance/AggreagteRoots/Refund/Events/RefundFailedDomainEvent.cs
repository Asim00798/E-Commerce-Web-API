using E_Commerce.Domain.SharedKernel.Events;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Events;

public sealed class RefundFailedDomainEvent : DomainEvent
{
    public RefundFailedDomainEvent(
        Guid refundId,
        Guid paymentId,
        Money amount,
        string? reason)
    {
        RefundId = refundId;
        PaymentId = paymentId;
        Amount = amount;
        Reason = reason;
    }

    public Guid RefundId { get; }
    public Guid PaymentId { get; }
    public Money Amount { get; }
    public string? Reason { get; }
}