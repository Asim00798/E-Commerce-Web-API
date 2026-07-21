using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.Shared.Communication.Notifications.Models;

/// <summary>
/// Typed model for a welcome email after registration.
/// </summary>
public sealed class WelcomeEmail : INotificationModel
{
    public string TemplateName => "Welcome";

    public string RecipientEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
}