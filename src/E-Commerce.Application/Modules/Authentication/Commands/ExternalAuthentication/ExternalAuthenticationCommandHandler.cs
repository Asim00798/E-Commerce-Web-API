using E_Commerce.Application.Modules.Authentication.Abstractions;
using E_Commerce.Application.Modules.Authentication.Dtos;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authentication.Commands.ExternalAuthentication;

/// <summary>
/// Handles external provider authentication by delegating to <see cref="IAuthenticationService"/>.
/// </summary>
public sealed class ExternalAuthenticationCommandHandler
    : IRequestHandler<ExternalAuthenticationCommand, Result<AuthenticationResultDto>>
{
    private readonly IAuthenticationService _authenticationService;

    public ExternalAuthenticationCommandHandler(IAuthenticationService authenticationService)
        => _authenticationService = authenticationService;

    public async Task<Result<AuthenticationResultDto>> Handle(
        ExternalAuthenticationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.ExternalAuthenticateAsync(
            command.Provider,
            command.SubjectId,
            cancellationToken);

        return result.Succeeded
            ? Result<AuthenticationResultDto>.Success(result)
            : Result<AuthenticationResultDto>.Failure(result.Errors);
    }
}