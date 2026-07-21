using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.Shared.Communication.Notifications.Models;

/// <summary>
/// Typed model for an SMS shipment notification.
/// </summary>
public sealed class OrderShippedSms : INotificationModel
{
    public string TemplateName => "OrderShipped";

    public string PhoneNumber { get; init; } = string.Empty;
    public Guid OrderId { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
}