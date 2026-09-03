using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.Modules.Identity.Credentials.IntegrationEvents;

/// <summary>
/// Published when a user requests a password reset.
/// Contains sensitive reset token; protect as sensitive payload.
/// </summary>
public sealed record PasswordResetRequestedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }

    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string ResetToken { get; init; } = string.Empty;
}