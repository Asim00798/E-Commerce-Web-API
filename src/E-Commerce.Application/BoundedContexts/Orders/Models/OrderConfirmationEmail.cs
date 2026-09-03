using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.Models;

/// <summary>
/// Typed model for an order confirmation email.
/// </summary>
public sealed class OrderConfirmationEmail : IEmailNotificationModel
{
    public string RecipientEmail { get; init; } = string.Empty;
    public string Subject => "Order Confirmed";
    public string CustomerName { get; init; } = string.Empty;
    public Guid OrderId { get; init; }
    public decimal Total { get; init; }
    public string TemplateName => "OrderConfirmation";
}