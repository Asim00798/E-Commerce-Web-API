using E_Commerce.Application.BoundedContexts.Shipping.Commands.AssignDriver;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.CancelShipment;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.ReassignDriver;
using E_Commerce.Application.BoundedContexts.Shipping.Queries.GetShipmentById;
using E_Commerce.Api.DTOs.Shipping.Requests;
using E_Commerce.Api.DTOs.Shipping.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Shipping;

[ApiController]
[Route("api/dispatcher/shipments")]
[Authorize(Roles = "Dispatcher,Administrator")]
public sealed class DispatcherShipmentsController : BaseApiController
{
    private const string AssignPolicy = "Permission:Shipments.Assign";
    private const string ManagePolicy = "Permission:Shipments.Manage";
    private const string ReadPolicy = "Permission:Shipments.Read";

    private readonly ISender _sender;

    public DispatcherShipmentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{shipmentId:guid}")]
    [Authorize(Policy = ReadPolicy)]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipment(
        Guid shipmentId,
        CancellationToken ct)
    {
        var query = new GetShipmentByIdQuery(shipmentId);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return NotFound(result.Errors);

        return Ok(MapToResponse(result.Data!));
    }

    [HttpPost("{shipmentId:guid}/assign-driver")]
    [Authorize(Policy = AssignPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignDriver(
        Guid shipmentId,
        [FromBody] AssignDriverRequest request,
        CancellationToken ct)
    {
        var command = new AssignDriverCommand(shipmentId, request.DriverId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/reassign-driver")]
    [Authorize(Policy = AssignPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReassignDriver(
        Guid shipmentId,
        [FromBody] ReassignDriverRequest request,
        CancellationToken ct)
    {
        var command = new ReassignDriverCommand(shipmentId, request.NewDriverId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{orderId:guid}/cancel")]
    [Authorize(Policy = ManagePolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelShipment(
        Guid orderId,
        CancellationToken ct)
    {
        var command = new CancelShipmentCommand(orderId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    private static ShipmentResponse MapToResponse(
        E_Commerce.Application.BoundedContexts.Shipping.Dtos.ShipmentDto dto)
    {
        return new ShipmentResponse
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
    }
}