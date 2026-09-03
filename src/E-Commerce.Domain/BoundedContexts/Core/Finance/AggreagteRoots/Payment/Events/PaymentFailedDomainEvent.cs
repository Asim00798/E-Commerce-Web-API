using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Finance.AggregateRoots.Payment.Events;

public sealed class PaymentFailedDomainEvent : DomainEvent
{
    public PaymentFailedDomainEvent(Guid paymentId, Guid orderId)
    {
        PaymentId = paymentId;
        OrderId = orderId;
    }

    public Guid PaymentId { get; }
    public Guid OrderId { get; }
}