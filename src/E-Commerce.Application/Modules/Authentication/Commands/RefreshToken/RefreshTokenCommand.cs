using E_Commerce.Application.Modules.Authentication.Dtos;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authentication.Commands.RefreshToken;

/// <summary>
/// Command to exchange a refresh token for a new token pair.
/// </summary>
public sealed record RefreshTokenCommand(
    string RefreshToken) : IRequest<Result<AuthenticationResultDto>>;