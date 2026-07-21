using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.Shared.Communication.Notifications.Models;

/// <summary>
/// Typed model for a password reset email.
/// </summary>
public sealed class PasswordResetEmail : INotificationModel
{
    public string TemplateName => "PasswordReset";

    public string RecipientEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string ResetToken { get; init; } = string.Empty;
    public DateTime ExpiryDate { get; init; }
}