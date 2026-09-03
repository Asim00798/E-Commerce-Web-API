using E_Commerce.Application.Modules.Authentication.Abstractions;
using E_Commerce.Application.Modules.Authentication.Dtos;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authentication.Commands.Login;

/// <summary>
/// Handles local login by delegating to <see cref="IAuthenticationService"/>.
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthenticationResultDto>>
{
    private readonly IAuthenticationService _authenticationService;

    public LoginCommandHandler(IAuthenticationService authenticationService)
        => _authenticationService = authenticationService;

    public async Task<Result<AuthenticationResultDto>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LoginAsync(
            command.Email,
            command.Password,
            cancellationToken);

        return result.Succeeded
            ? Result<AuthenticationResultDto>.Success(result)
            : Result<AuthenticationResultDto>.Failure(result.Errors);
    }
}