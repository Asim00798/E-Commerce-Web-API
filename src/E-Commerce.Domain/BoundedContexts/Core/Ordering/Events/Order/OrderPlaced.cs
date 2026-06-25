using Domain.SharedKernel.Events;
using E_Commerce.Domain.SharedKernel.Events;

namespace Domain.Orders.Events;

public class OrderPlacedDomainEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }
    public DateTime OccurredAt { get; }

    public OrderPlacedDomainEvent(Guid orderId, Guid customerId, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        OccurredAt = DateTime.UtcNow;
    }
}