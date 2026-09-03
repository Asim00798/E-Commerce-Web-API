using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Events;

public sealed class OrderPaidDomainEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }

    public OrderPaidDomainEvent(Guid orderId, Guid customerId)
    {
        OrderId = orderId;
        CustomerId = customerId;
    }
}