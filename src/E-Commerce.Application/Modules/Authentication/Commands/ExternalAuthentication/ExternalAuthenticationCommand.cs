using E_Commerce.Application.Modules.Authentication.Dtos;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authentication.Commands.ExternalAuthentication;

/// <summary>
/// Command to authenticate with an external identity provider.
/// The subject ID must come from a validated external principal.
/// </summary>
public sealed record ExternalAuthenticationCommand(
    string Provider,
    string SubjectId) : IRequest<Result<AuthenticationResultDto>>;