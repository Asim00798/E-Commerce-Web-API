using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEvents;

/// <summary>
/// Published when a registration has been fully verified.
/// Triggers account provisioning.
/// </summary>
public sealed record RegistrationFullyVerifiedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid RegistrationId { get; init; }
    public string Email { get; init; }
    public string PhoneNumber { get; init; }
    public string Username { get; init; }

    public RegistrationFullyVerifiedIntegrationEvent(
        Guid registrationId, string email, string phoneNumber, string username)
    {
        RegistrationId = registrationId;
        Email = email;
        PhoneNumber = phoneNumber;
        Username = username;
    }
}