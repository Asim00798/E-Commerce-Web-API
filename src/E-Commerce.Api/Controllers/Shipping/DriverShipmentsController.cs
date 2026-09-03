using E_Commerce.Api.DTOs.Shipping.Requests;
using E_Commerce.Api.DTOs.Shipping.Responses;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.BeginReturn;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.MarkPickedUp;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.RecordDeliveryAttempt;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.RetryDelivery;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.StartDelivery;
using E_Commerce.Application.BoundedContexts.Shipping.Queries.GetDriverShipments;
using E_Commerce.Infrastructure.Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Shipping;

[ApiController]
[Route("api/driver/shipments")]
[Authorize(Roles = "Driver")]
public sealed class DriverShipmentsController : BaseApiController
{
    private const string DeliverPolicy = "Permission:Shipments.Deliver";

    private readonly ISender _sender;

    public DriverShipmentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = DeliverPolicy)]
    [ProducesResponseType(typeof(IReadOnlyList<ShipmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyShipments(CancellationToken ct)
    {
        var driverId = CurrentUserId;
        if (driverId is null)
            return Unauthorized();

        var query = new GetDriverShipmentsQuery(driverId.Value);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var response = result.Data!.Select(MapToResponse).ToList();
        return Ok(response);
    }

    [HttpPost("{shipmentId:guid}/pickup")]
    [Authorize(Policy = DeliverPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkPickedUp(
        Guid shipmentId,
        CancellationToken ct)
    {
        var command = new MarkPickedUpCommand(shipmentId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/start-delivery")]
    [Authorize(Policy = DeliverPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartDelivery(
        Guid shipmentId,
        CancellationToken ct)
    {
        var command = new StartDeliveryCommand(shipmentId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/delivery-attempt")]
    [Authorize(Policy = DeliverPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/retry")]
    [Authorize(Policy = DeliverPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RetryDelivery(
        Guid shipmentId,
        CancellationToken ct)
    {
        var command = new RetryDeliveryCommand(shipmentId);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpPost("{shipmentId:guid}/begin-return")]
    [Authorize(Policy = DeliverPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BeginReturn(
        Guid shipmentId,
        CancellationToken ct)
    {
        var command = new BeginReturnCommand(shipmentId);
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