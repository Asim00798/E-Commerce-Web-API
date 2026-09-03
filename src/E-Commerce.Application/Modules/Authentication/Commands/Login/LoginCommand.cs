using E_Commerce.Application.Modules.Authentication.Dtos;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authentication.Commands.Login;

/// <summary>
/// Command to log in with email and password.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<Result<AuthenticationResultDto>>;