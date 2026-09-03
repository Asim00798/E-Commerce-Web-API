using MediatR;
using E_Commerce.Application.Shared.Models;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Commands.ResendEmail;

/// <summary>
/// Command to request a new email verification code.
/// </summary>
public sealed record ResendEmailCommand : IRequest<Result>
{
    public Guid RegistrationId { get; init; }
}