using E_Commerce.Domain.SharedKernel.Events;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Events;

public sealed class RefundFailedDomainEvent : DomainEvent
{
    public Guid RefundId { get; }
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public Money Amount { get; }
    public string? Reason { get; }

    public RefundFailedDomainEvent(
        Guid refundId,
        Guid paymentId,
        Guid orderId,
        Money amount,
        string? reason = null)
    {
        RefundId = refundId;
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        Reason = reason;
    }
}