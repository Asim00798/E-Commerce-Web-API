using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEvents;

/// <summary>
/// Published when an email verification code is generated for a registration.
/// Contains only the data necessary for the notification channel.
/// </summary>
public sealed record EmailVerificationRequestedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid RegistrationId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}