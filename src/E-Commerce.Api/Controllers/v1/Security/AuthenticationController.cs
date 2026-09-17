using System.Security.Claims;
using E_Commerce.Api.Controllers.Common;
using E_Commerce.Application.Modules.Authentication.Abstractions;
using E_Commerce.Application.Modules.Authentication.Commands.ExternalAuthentication;
using E_Commerce.Application.Modules.Authentication.Commands.LinkGoogle;
using E_Commerce.Application.Modules.Authentication.Constants;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.v1.Security;

/// <summary>
/// Handles external authentication flows: Google sign-in and Google account linking.
/// Callback endpoints validate the external principal and delegate token issuance
/// or account linking to the Application layer.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IUserLinkStateProtector _stateProtector;

    public AuthenticationController(
        IMediator mediator,
        IUserLinkStateProtector stateProtector)
    {
        _mediator = mediator;
        _stateProtector = stateProtector;
    }

    // ------------------------------------------------------------------
    // Google sign-in
    // ------------------------------------------------------------------

    /// <summary>
    /// Starts the Google OAuth challenge for sign-in. Redirects the browser to
    /// Google, which returns to the callback action.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("external/google")]
    public IActionResult ExternalLoginGoogle()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/auth/external/google/callback"
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Handles the Google OAuth callback for sign-in. Validates the external
    /// principal and issues application tokens via
    /// <see cref="ExternalAuthenticationCommand"/>.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("external/google/callback")]
    public async Task<IActionResult> ExternalLoginGoogleCallback(
        CancellationToken cancellationToken)
    {
        try
        {
            var authResult = await HttpContext.AuthenticateAsync(
                AuthenticationConstants.ExternalCookieScheme);

            if (!authResult.Succeeded || authResult.Principal is null)
                return Unauthorized("External authentication failed.");

            var subjectId = GetGoogleSubjectId(authResult.Principal);

            if (subjectId is null)
                return BadRequest("Google subject claim missing.");

            var command = new ExternalAuthenticationCommand(
                AuthenticationConstants.GoogleProvider,
                subjectId);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Succeeded)
                return Unauthorized(result.Errors.FirstOrDefault());

            return Ok(result.Data);
        }
        finally
        {
            await ClearExternalCookieAsync();
        }
    }

    // ------------------------------------------------------------------
    // Google account linking
    // ------------------------------------------------------------------

    /// <summary>
    /// Starts the Google OAuth challenge to link the currently authenticated user
    /// with a Google account. The user ID is protected with Data Protection before
    /// being carried in OAuth state.
    /// </summary>
    [Authorize]
    [HttpGet("link/google")]
    public IActionResult LinkGoogle()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var protectedUserId = _stateProtector.Protect(userId);

        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/auth/link/google/callback"
        };

        properties.Items[AuthenticationConstants.LinkUserIdKey] = protectedUserId;

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Handles the Google OAuth callback for account linking. Validates the external
    /// principal and the protected user ID from OAuth state, then links the Google
    /// subject via <see cref="LinkGoogleForUserCommand"/>.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("link/google/callback")]
    public async Task<IActionResult> LinkGoogleCallback(
        CancellationToken cancellationToken)
    {
        try
        {
            var authResult = await HttpContext.AuthenticateAsync(
                AuthenticationConstants.ExternalCookieScheme);

            if (!authResult.Succeeded || authResult.Principal is null)
                return Unauthorized("Google authentication failed.");

            var subjectId = GetGoogleSubjectId(authResult.Principal);

            if (subjectId is null)
                return BadRequest("Google subject claim missing.");

            if (!authResult.Properties.Items.TryGetValue(
                    AuthenticationConstants.LinkUserIdKey,
                    out var protectedUserId))
            {
                return BadRequest("Google linking state is invalid.");
            }

            var command = new LinkGoogleForUserCommand(subjectId, protectedUserId!);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Succeeded)
                return BadRequest(result.Errors.FirstOrDefault());

            return Ok();
        }
        finally
        {
            await ClearExternalCookieAsync();
        }
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Extracts the Google subject identifier (mapped to <see cref="ClaimTypes.NameIdentifier"/>)
    /// from the external principal. Returns null when the claim is missing or blank.
    /// </summary>
    private static string? GetGoogleSubjectId(ClaimsPrincipal principal)
    {
        var subjectId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrWhiteSpace(subjectId) ? null : subjectId;
    }

    /// <summary>
    /// Signs out of the temporary external authentication cookie.
    /// </summary>
    private Task ClearExternalCookieAsync()
        => HttpContext.SignOutAsync(
            AuthenticationConstants.ExternalCookieScheme);
}