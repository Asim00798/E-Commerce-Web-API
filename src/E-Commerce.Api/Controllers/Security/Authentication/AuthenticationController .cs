using System.Security.Claims;
using E_Commerce.Application.Modules.Authentication.Abstractions;
using E_Commerce.Application.Modules.Authentication.Commands.ExternalAuthentication;
using E_Commerce.Application.Modules.Authentication.Commands.LinkGoogle;
using E_Commerce.Application.Modules.Authentication.Constants;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Security.Authentication;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController : ControllerBase
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

    // ---------------------------------------------------------------
    // Google Login
    // ---------------------------------------------------------------

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

    [AllowAnonymous]
    [HttpGet("external/google/callback")]
    public async Task<IActionResult> ExternalLoginGoogleCallback(
        CancellationToken cancellationToken)
    {
        var authResult = await HttpContext.AuthenticateAsync(
            AuthenticationConstants.ExternalCookieScheme);

        if (!authResult.Succeeded || authResult.Principal is null)
        {
            await ClearExternalCookieAsync();
            return Unauthorized("External authentication failed.");
        }

        try
        {
            var subjectId = authResult.Principal.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(subjectId))
            {
                return BadRequest("Google subject claim missing.");
            }

            var command = new ExternalAuthenticationCommand(
                AuthenticationConstants.GoogleProvider,
                subjectId);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Succeeded)
            {
                return Unauthorized(result.Errors.FirstOrDefault());
            }

            return Ok(result.Data);
        }
        finally
        {
            await ClearExternalCookieAsync();
        }
    }

    // ---------------------------------------------------------------
    // Google Linking
    // ---------------------------------------------------------------

    [Authorize]
    [HttpGet("link/google")]
    public IActionResult LinkGoogle()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        // Protect the user ID before placing it in OAuth state.
        var protectedUserId = _stateProtector.Protect(userId);

        var properties = new AuthenticationProperties
        {
            RedirectUri = "/api/auth/link/google/callback"
        };

        properties.Items[AuthenticationConstants.LinkUserIdKey] = protectedUserId;

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    [HttpGet("link/google/callback")]
    public async Task<IActionResult> LinkGoogleCallback(
        CancellationToken cancellationToken)
    {
        var authResult = await HttpContext.AuthenticateAsync(
            AuthenticationConstants.ExternalCookieScheme);

        if (!authResult.Succeeded || authResult.Principal is null)
        {
            await ClearExternalCookieAsync();
            return Unauthorized("Google authentication failed.");
        }

        try
        {
            var subjectId = authResult.Principal.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(subjectId))
            {
                return BadRequest("Google subject claim missing.");
            }

            if (!authResult.Properties.Items.TryGetValue(
                    AuthenticationConstants.LinkUserIdKey,
                    out var protectedUserId))
            {
                return BadRequest("Google linking state is invalid.");
            }

            var command = new LinkGoogleForUserCommand(
                subjectId,
                protectedUserId!);

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.FirstOrDefault());
            }

            return Ok();
        }
        finally
        {
            await ClearExternalCookieAsync();
        }
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    private Task ClearExternalCookieAsync()
        => HttpContext.SignOutAsync(
            AuthenticationConstants.ExternalCookieScheme);
}