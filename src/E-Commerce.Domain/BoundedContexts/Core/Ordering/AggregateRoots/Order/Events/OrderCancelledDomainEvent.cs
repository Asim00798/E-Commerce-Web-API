using E_Commerce.Domain.SharedKernel.Events;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Events;

public sealed class OrderCancelledDomainEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }

    public OrderCancelledDomainEvent(Guid orderId, Guid customerId)
    {
        OrderId = orderId;
        CustomerId = customerId;
    }
}