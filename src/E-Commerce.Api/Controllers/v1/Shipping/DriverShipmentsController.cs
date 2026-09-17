using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Shipping.Requests;
using E_Commerce.Api.DTOs.v1.Shipping.Responses;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.BeginReturn;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.MarkPickedUp;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.RecordDeliveryAttempt;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.RetryDelivery;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.StartDelivery;
using E_Commerce.Application.BoundedContexts.Shipping.Dtos;
using E_Commerce.Application.BoundedContexts.Shipping.Queries.GetDriverShipments;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.Shipping;

/// <summary>
/// Driver-facing shipment API. Drivers can view and operate only on shipments
/// assigned to them. Assignment ownership is enforced by the command handlers.
/// </summary>
[ApiController]
[Route("api/driver/shipments")]
[Authorize(Roles = $"{SystemRoles.Driver}")]
public sealed class DriverShipmentsController : BaseApiController
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public DriverShipmentsController(ISender sender, ICurrentUser currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Gets the shipments currently assigned to the authenticated driver.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ShipmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyShipments(CancellationToken ct)
    {
        var driverId = _currentUser.UserId;

        if (driverId is null)
            return Unauthorized();

        var result = await _sender.Send(new GetDriverShipmentsQuery(driverId.Value), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        var response = result.Data!.Select(MapToResponse).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Marks a shipment as picked up by the assigned driver.
    /// </summary>
    [HttpPost("{shipmentId:guid}/pickup")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkPickedUp(Guid shipmentId, CancellationToken ct)
    {
        var result = await _sender.Send(new MarkPickedUpCommand(shipmentId), ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    /// <summary>
    /// Starts delivery for a shipment assigned to the authenticated driver.
    /// </summary>
    [HttpPost("{shipmentId:guid}/start-delivery")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartDelivery(Guid shipmentId, CancellationToken ct)
    {
        var result = await _sender.Send(new StartDeliveryCommand(shipmentId), ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    /// <summary>
    /// Records a delivery attempt (successful or failed) for a shipment assigned
    /// to the authenticated driver.
    /// </summary>
    [HttpPost("{shipmentId:guid}/delivery-attempt")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordDeliveryAttempt(
        Guid shipmentId,
        [FromBody] RecordDeliveryAttemptRequest request,
        CancellationToken ct)
    {
        var command = new RecordDeliveryAttemptCommand(
            shipmentId,
            request.Result,
            request.FailureReason,
            request.Notes);

        var result = await _sender.Send(command, ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    /// <summary>
    /// Retries delivery after a failed attempt, if allowed by the maximum-attempts
    /// policy.
    /// </summary>
    [HttpPost("{shipmentId:guid}/retry")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RetryDelivery(Guid shipmentId, CancellationToken ct)
    {
        var result = await _sender.Send(new RetryDeliveryCommand(shipmentId), ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    /// <summary>
    /// Begins the return-to-sender flow for a shipment assigned to the
    /// authenticated driver.
    /// </summary>
    [HttpPost("{shipmentId:guid}/begin-return")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BeginReturn(Guid shipmentId, CancellationToken ct)
    {
        var result = await _sender.Send(new BeginReturnCommand(shipmentId), ct);

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
}