using E_Commerce.Domain.SharedKernel.Events;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Events;

public sealed class RefundCompletedDomainEvent : DomainEvent
{
    public RefundCompletedDomainEvent(
        Guid refundId,
        Guid paymentId,
        Money amount)
    {
        RefundId = refundId;
        PaymentId = paymentId;
        Amount = amount;
    }

    public Guid RefundId { get; }
    public Guid PaymentId { get; }
    public Money Amount { get; }
}