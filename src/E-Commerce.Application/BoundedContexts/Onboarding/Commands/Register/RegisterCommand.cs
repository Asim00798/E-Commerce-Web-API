using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Commands.Register;

/// <summary>
/// Command to initiate a new user registration.
/// </summary>
public sealed record RegisterCommand : IRequest<Result<Guid>>
{
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}