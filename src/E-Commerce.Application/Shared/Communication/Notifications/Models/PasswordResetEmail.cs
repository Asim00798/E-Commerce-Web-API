using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.Shared.Communication.Notifications.Models;

/// <summary>
/// Typed model for a password reset email.
/// </summary>
public sealed class PasswordResetEmail : IEmailNotificationModel
{
    public string RecipientEmail { get; init; } = string.Empty;
    public string Subject => "Password Reset Request";
    public string CustomerName { get; init; } = string.Empty;
    public string ResetToken { get; init; } = string.Empty;
    public DateTime ExpiryDate { get; init; }
    public string TemplateName => "PasswordReset";
}