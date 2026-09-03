using E_Commerce.Domain.SharedKernel.Events;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Events;

public sealed class OrderPlacedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public Money TotalAmount { get; }

    public OrderPlacedDomainEvent(Guid orderId, Guid customerId
        , Money totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}