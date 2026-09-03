using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Models;

public sealed class RegistrationVerificationSms : ISmsNotificationModel
{
    public string PhoneNumber { get; init; } = string.Empty;
    public string Text => $"Your verification code is: {VerificationCode}";
    public string VerificationCode { get; init; } = string.Empty;
    public string TemplateName => "RegistrationVerification";
}