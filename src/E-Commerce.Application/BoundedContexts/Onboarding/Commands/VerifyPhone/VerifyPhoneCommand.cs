using MediatR;
using E_Commerce.Application.Shared.Models;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Commands.VerifyPhone;

/// <summary>
/// Command to verify the phone number of a pending registration.
/// </summary>
public sealed record VerifyPhoneCommand : IRequest<Result>
{
    public Guid RegistrationId { get; init; }
    public string Code { get; init; } = string.Empty;
}