using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.Shared.Communication.Notifications.Models;

/// <summary>
/// Typed model for an order confirmation email.
/// </summary>
public sealed class OrderConfirmationEmail : INotificationModel
{
    public string TemplateName => "OrderConfirmation";

    public string RecipientEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public Guid OrderId { get; init; }
    public decimal Total { get; init; }
}