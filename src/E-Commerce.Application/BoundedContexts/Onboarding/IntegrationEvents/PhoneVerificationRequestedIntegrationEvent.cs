using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEvents;

/// <summary>
/// Published when a phone verification code is generated for a registration.
/// </summary>
public sealed record PhoneVerificationRequestedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid RegistrationId { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}