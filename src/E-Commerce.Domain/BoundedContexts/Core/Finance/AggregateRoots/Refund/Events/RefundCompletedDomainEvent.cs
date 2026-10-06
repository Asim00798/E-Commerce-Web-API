using E_Commerce.Domain.SharedKernel.Events;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Refund.Events;

public sealed class RefundCompletedDomainEvent : DomainEvent
{
    public Guid RefundId { get; }
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public Money Amount { get; }

    public RefundCompletedDomainEvent(
        Guid refundId,
        Guid paymentId,
        Guid orderId,
        Money amount)
    {
        RefundId = refundId;
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
    }
}