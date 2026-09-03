using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;

public sealed class OrderReadyForShippingIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public string FullName { get; }
    public string PhoneNumber { get; }
    public string Street { get; }
    public string City { get; }
    public string LocationMapUrl { get; }

    public OrderReadyForShippingIntegrationEvent(
        Guid orderId,
        Guid customerId,
        string fullName,
        string phoneNumber,
        string street,
        string city,
        string locationMapUrl)
    {
        OrderId = orderId;
        CustomerId = customerId;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        Street = street;
        City = city;
        LocationMapUrl = locationMapUrl;
    }
}