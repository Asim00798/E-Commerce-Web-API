using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Models;

public sealed class RegistrationVerificationEmail : IEmailNotificationModel
{
    public string RecipientEmail { get; init; } = string.Empty;
    public string Subject => "Verify your email address";
    public string VerificationCode { get; init; } = string.Empty;
    public string TemplateName => "RegistrationVerification";
}