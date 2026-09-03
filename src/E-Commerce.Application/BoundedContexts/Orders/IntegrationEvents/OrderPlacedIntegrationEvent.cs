using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;

public sealed class OrderPlacedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }
    public string Currency { get; }
    public string CustomerEmail { get; }
    public string CustomerName { get; }

    public OrderPlacedIntegrationEvent(
        Guid orderId,
        Guid customerId,
        decimal totalAmount,
        string currency,
        string customerEmail,
        string customerName)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Currency = currency;
        CustomerEmail = customerEmail;
        CustomerName = customerName;
    }
}