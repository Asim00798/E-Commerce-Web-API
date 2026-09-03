using E_Commerce.Application.Modules.Authentication.Abstractions;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authentication.Commands.Logout;

/// <summary>
/// Handles logout by delegating to <see cref="IAuthenticationService"/>.
/// </summary>
public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IAuthenticationService _authenticationService;

    public LogoutCommandHandler(IAuthenticationService authenticationService)
        => _authenticationService = authenticationService;

    public async Task<Result> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        await _authenticationService.LogoutAsync(
            command.RefreshToken,
            cancellationToken);

        return Result.Success();
    }
}