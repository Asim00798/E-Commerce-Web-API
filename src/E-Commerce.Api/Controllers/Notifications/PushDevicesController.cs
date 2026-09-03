using E_Commerce.Application.Modules.Notifications.Commands.RegisterPushDevice;
using E_Commerce.Application.Modules.Notifications.Commands.DeactivatePushDevice;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce.Api.Controllers.Notifications;

[ApiController]
[Route("api/push-devices")]
[Authorize]
public class PushDevicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PushDevicesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterPushDeviceRequest request)
    {
        var command = new RegisterPushDeviceCommand(
            GetCurrentUserId(),
            request.FirebaseInstallationId,
            request.Platform);

        var deviceId = await _mediator.Send(command);
        return Ok(new { DeviceId = deviceId });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _mediator.Send(new DeactivatePushDeviceCommand(id));
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null ? Guid.Parse(claim.Value) : Guid.Empty;
    }
}
public record RegisterPushDeviceRequest(string FirebaseInstallationId, string Platform);