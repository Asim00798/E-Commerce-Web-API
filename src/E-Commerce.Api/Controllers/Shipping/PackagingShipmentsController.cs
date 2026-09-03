using E_Commerce.Application.BoundedContexts.Shipping.Commands.CompleteReturn;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.MarkReadyForPickup;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Shipping;

[ApiController]
[Route("api/packaging/shipments")]
[Authorize(Roles = "Packaging")]
public sealed class PackagingShipmentsController : BaseApiController
{
    private const string ManagePolicy = "Permission:Shipments.Manage";

    private readonly ISender _sender;

    public PackagingShipmentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{shipmentId:guid}/ready-for-pickup")]
    [Authorize(Policy = ManagePolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkReadyForPickup(
        Guid shipmentId,
        CancellationToken ct)
    {
        var command = new MarkReadyForPickupCommand(shipmentId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/complete-return")]
    [Authorize(Policy = ManagePolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteReturn(
        Guid shipmentId,
        CancellationToken ct)
    {
        var command = new CompleteReturnCommand(shipmentId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }
}