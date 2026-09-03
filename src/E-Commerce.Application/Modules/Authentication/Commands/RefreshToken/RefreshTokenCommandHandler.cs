using E_Commerce.Application.Modules.Authentication.Abstractions;
using E_Commerce.Application.Modules.Authentication.Dtos;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authentication.Commands.RefreshToken;

/// <summary>
/// Handles refresh token flow by delegating to <see cref="IAuthenticationService"/>.
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResultDto>>
{
    private readonly IAuthenticationService _authenticationService;

    public RefreshTokenCommandHandler(IAuthenticationService authenticationService)
        => _authenticationService = authenticationService;

    public async Task<Result<AuthenticationResultDto>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RefreshAsync(
            command.RefreshToken,
            cancellationToken);

        return result.Succeeded
            ? Result<AuthenticationResultDto>.Success(result)
            : Result<AuthenticationResultDto>.Failure(result.Errors);
    }
}