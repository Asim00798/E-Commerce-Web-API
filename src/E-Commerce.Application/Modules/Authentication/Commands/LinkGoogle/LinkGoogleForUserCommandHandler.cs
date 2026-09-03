using E_Commerce.Application.Modules.Authentication.Abstractions;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authentication.Commands.LinkGoogle;

/// <summary>
/// Handles the Google linking callback by unprotecting the user ID and then
/// delegating the actual linking to <see cref="IAuthenticationService"/>.
/// </summary>
public sealed class LinkGoogleForUserCommandHandler
    : IRequestHandler<LinkGoogleForUserCommand, Result>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserLinkStateProtector _stateProtector;

    public LinkGoogleForUserCommandHandler(
        IAuthenticationService authenticationService,
        IUserLinkStateProtector stateProtector)
    {
        _authenticationService = authenticationService;
        _stateProtector = stateProtector;
    }

    public async Task<Result> Handle(
        LinkGoogleForUserCommand command,
        CancellationToken cancellationToken)
    {
        if (!_stateProtector.TryUnprotect(
                command.ProtectedUserId,
                out var userId))
        {
            return Result.Failure("Google linking state is invalid.");
        }

        await _authenticationService.LinkGoogleAsync(
            userId,
            command.SubjectId,
            cancellationToken);

        return Result.Success();
    }
}