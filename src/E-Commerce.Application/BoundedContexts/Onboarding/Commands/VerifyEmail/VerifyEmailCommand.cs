using MediatR;
using E_Commerce.Application.Shared.Models;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Commands.VerifyEmail;

/// <summary>
/// Command to verify the email address of a pending registration.
/// </summary>
public sealed record VerifyEmailCommand : IRequest<Result>
{
    public Guid RegistrationId { get; init; }
    public string Code { get; init; } = string.Empty;
}