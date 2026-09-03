using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Orders.Models;

public sealed class FeedbackRequestEmail : IEmailNotificationModel
{
    public string RecipientEmail { get; init; } = string.Empty;
    public string Subject => "How was your order?";
    public string CustomerName { get; init; } = string.Empty;
    public Guid OrderId { get; init; }
    public string TemplateName => "FeedbackRequest"; // matches the .cshtml template
}