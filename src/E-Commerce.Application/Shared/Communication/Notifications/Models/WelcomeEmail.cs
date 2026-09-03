using E_Commerce.Application.Shared.Communication.Notifications.Models;

namespace E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

/// <summary>
/// Typed model for a welcome email after registration.
/// </summary>
public sealed class WelcomeEmail : IEmailNotificationModel
{
    public string RecipientEmail { get; init; } = string.Empty;
    public string Subject => "Welcome!";
    public string CustomerName { get; init; } = string.Empty;
    public string TemplateName => "Welcome";
}