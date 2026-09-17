using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Notifications.Requests;
using E_Commerce.Application.Modules.Notifications.Commands.DeactivatePushDevice;
using E_Commerce.Application.Modules.Notifications.Commands.RegisterPushDevice;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce.Api.Controllers.v1.Notifications;

/// <summary>
/// Manages push notification devices registered by the authenticated user.
/// Every action requires authentication.
/// </summary>
[ApiController]
[Route("api/push-devices")]
[Authorize]
public sealed class PushDevicesController : BaseApiController
{
    private readonly ISender _sender;

    public PushDevicesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Registers a push notification device for the current user.
    /// Returns 200 with the new device identifier as a bare GUID string.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterPushDeviceRequest request,
        CancellationToken ct)
    {
        var command = new RegisterPushDeviceCommand(
            GetCurrentUserId(),
            request.FirebaseInstallationId,
            request.Platform);

        var deviceId = await _sender.Send(command, ct);

        return Ok(deviceId);
    }

    /// <summary>
    /// Deactivates a push notification device owned by the current user.
    /// Ownership is enforced by the command handler (see the handler/service
    /// implementation). Callers attempting to deactivate another user's device
    /// receive 204 (idempotent) — the device remains active because the
    /// ownership check prevents the operation.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeactivatePushDeviceCommand(id), ct);
        return NoContent();
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Resolves the current user's identifier from the authenticated principal.
    /// Throws if the claim is missing or malformed — this prevents silent
    /// queries against an empty GUID.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (claimValue is null || !Guid.TryParse(claimValue, out var userId))
            throw new UnauthorizedAccessException(
                "Authenticated principal is missing a valid NameIdentifier claim.");

        return userId;
    }
}