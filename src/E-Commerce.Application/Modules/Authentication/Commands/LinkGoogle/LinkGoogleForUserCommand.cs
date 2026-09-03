using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Authentication.Commands.LinkGoogle;

/// <summary>
/// Command used by the Google callback to link a Google external login
/// to an already identified local user.
/// The protected user ID comes from OAuth state; the subject ID from the validated Google principal.
/// </summary>
public sealed record LinkGoogleForUserCommand(
    string SubjectId,
    string ProtectedUserId) : IRequest<Result>;