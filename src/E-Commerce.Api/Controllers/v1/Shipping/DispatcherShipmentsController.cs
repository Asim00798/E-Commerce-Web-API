using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Shipping.Requests;
using E_Commerce.Api.DTOs.v1.Shipping.Responses;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.AssignDriver;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.CancelShipment;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.ReassignDriver;
using E_Commerce.Application.BoundedContexts.Shipping.Dtos;
using E_Commerce.Application.BoundedContexts.Shipping.Queries.GetShipmentById;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.Shipping;

/// <summary>
/// Dispatcher-facing shipment API. Dispatchers and administrators can view
/// shipments, assign or reassign drivers, and cancel shipments.
/// </summary>
[ApiController]
[Route("api/dispatcher/shipments")]
[Authorize(Roles = $"{SystemRoles.Dispatcher},{SystemRoles.Administrator}")]
public sealed class DispatcherShipmentsController : BaseApiController
{
    private readonly ISender _sender;

    public DispatcherShipmentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Gets a shipment by ID.
    /// </summary>
    [HttpGet("{shipmentId:guid}")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetShipment(
        Guid shipmentId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetShipmentByIdQuery(shipmentId), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(MapToResponse(result.Data!));
    }

    /// <summary>
    /// Assigns a driver to a shipment.
    /// </summary>
    [HttpPost("{shipmentId:guid}/assign-driver")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignDriver(
        Guid shipmentId,
        [FromBody] AssignDriverRequest request,
        CancellationToken ct)
    {
        var command = new AssignDriverCommand(shipmentId, request.DriverId);
        var result = await _sender.Send(command, ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    /// <summary>
    /// Reassigns a shipment to a different driver.
    /// </summary>
    [HttpPost("{shipmentId:guid}/reassign-driver")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReassignDriver(
        Guid shipmentId,
        [FromBody] ReassignDriverRequest request,
        CancellationToken ct)
    {
        var command = new ReassignDriverCommand(shipmentId, request.NewDriverId);
        var result = await _sender.Send(command, ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    /// <summary>
    /// Cancels a shipment. The identifier and route segment are pending
    /// confirmation — see the review notes on whether this action should take
    /// a shipment ID or an order ID.
    /// </summary>
    [HttpPost("{orderId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelShipment(
        Guid orderId,
        CancellationToken ct)
    {
        var command = new CancelShipmentCommand(orderId);
        var result = await _sender.Send(command, ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    // ------------------------------------------------------------------
    // Mapping
    // ------------------------------------------------------------------

    private static ShipmentResponse MapToResponse(ShipmentDto dto) => new()
    {
        ShipmentId = dto.ShipmentId,
        OrderId = dto.OrderId,
        CustomerId = dto.CustomerId,
        Status = dto.Status,
        TrackingNumber = dto.TrackingNumber,
        AssignedDriverId = dto.AssignedDriverId,
        FullName = dto.FullName,
        PhoneNumber = dto.PhoneNumber,
        Street = dto.Street,
        City = dto.City,
        LocationMapUrl = dto.LocationMapUrl,
        DeliveryAttempts = dto.DeliveryAttempts
            .Select(x => new DeliveryAttemptResponse
            {
                AttemptNumber = x.AttemptNumber,
                AttemptedAtUtc = x.AttemptedAtUtc,
                Result = x.Result,
                FailureReason = x.FailureReason,
                Notes = x.Notes
            })
            .ToList()
    };

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

    private IActionResult ToNotFoundProblem(string[] errors) =>
        Problem(
            title: "Resource not found.",
            detail: string.Join(" ", errors),
            statusCode: StatusCodes.Status404NotFound);
}