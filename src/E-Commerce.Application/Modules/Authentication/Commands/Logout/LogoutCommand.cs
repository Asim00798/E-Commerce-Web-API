using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authentication.Commands.Logout;

/// <summary>
/// Command to revoke the current refresh token.
/// </summary>
public sealed record LogoutCommand(
    string RefreshToken) : IRequest<Result>;