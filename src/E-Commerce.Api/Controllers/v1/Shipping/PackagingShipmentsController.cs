using E_Commerce.Api.Controllers.Common;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.CompleteReturn;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.MarkReadyForPickup;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.Shipping;

/// <summary>
/// Packaging / warehouse-facing shipment API.
/// Packaging staff handle shipments at the fulfillment end — marking them ready
/// for pickup and completing the return flow when a shipment comes back.
/// There is no per-user ownership model for this role; the Shipment state machine
/// rejects operations that are not valid for the shipment's current state.
/// </summary>
[ApiController]
[Route("api/packaging/shipments")]
[Authorize(Roles = $"{SystemRoles.Packaging}")]
public sealed class PackagingShipmentsController : BaseApiController
{
    private readonly ISender _sender;

    public PackagingShipmentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Marks a shipment ready for pickup. Only valid from the <c>Assigned</c> state.
    /// </summary>
    [HttpPost("{shipmentId:guid}/ready-for-pickup")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkReadyForPickup(
        Guid shipmentId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new MarkReadyForPickupCommand(shipmentId), ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    /// <summary>
    /// Completes the return-to-sender flow. Only valid from the
    /// <c>ReturnToSender</c> state.
    /// </summary>
    [HttpPost("{shipmentId:guid}/complete-return")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteReturn(
        Guid shipmentId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new CompleteReturnCommand(shipmentId), ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    // ------------------------------------------------------------------
    // Error helpers
    // ------------------------------------------------------------------

    private IActionResult ToValidationProblem(string[] errors)
    {
        var modelState = new ModelStateDictionary();
        foreach (var error in errors)
        {
            modelState.AddModelError(string.Empty, error);
        }
        return ValidationProblem(modelState);
    }
}