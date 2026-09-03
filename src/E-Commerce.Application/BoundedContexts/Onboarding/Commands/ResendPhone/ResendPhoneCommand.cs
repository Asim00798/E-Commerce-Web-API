using MediatR;
using E_Commerce.Application.Shared.Models;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Commands.ResendPhone;

/// <summary>
/// Command to request a new phone verification code.
/// </summary>
public sealed record ResendPhoneCommand : IRequest<Result>
{
    public Guid RegistrationId { get; init; }
}