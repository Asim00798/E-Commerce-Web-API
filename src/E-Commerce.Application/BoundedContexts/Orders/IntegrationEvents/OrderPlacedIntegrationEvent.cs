using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents
{
    public class OrderPlacedIntegrationEvent : IIntegrationEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public decimal TotalAmount { get; }

        public OrderPlacedIntegrationEvent(Guid orderId, Guid customerId, decimal totalAmount)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            OrderId = orderId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
        }
    }
}
