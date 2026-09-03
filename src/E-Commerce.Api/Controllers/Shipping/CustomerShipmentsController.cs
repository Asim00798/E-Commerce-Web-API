using E_Commerce.Api.DTOs.Shipping.Responses;
using E_Commerce.Application.BoundedContexts.Shipping.Commands.CancelShipment;
using E_Commerce.Application.BoundedContexts.Shipping.Queries.GetCustomerShipments;
using E_Commerce.Application.BoundedContexts.Shipping.Queries.GetShipmentById;
using E_Commerce.Application.BoundedContexts.Shipping.Queries.GetShipmentByOrderId;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Infrastructure.Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Shipping;

[ApiController]
[Route("api/customer/shipments")]
[Authorize(Roles = "Customer")]
public sealed class CustomerShipmentsController : BaseApiController
{
    private const string ReadPolicy = "Permission:Shipments.Read";

    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public CustomerShipmentsController(ISender sender, ICurrentUser currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    [HttpGet("{shipmentId:guid}")]
    [Authorize(Policy = ReadPolicy)]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipmentById(
        Guid shipmentId,
        CancellationToken ct)
    {
        var query = new GetShipmentByIdQuery(shipmentId);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return NotFound(result.Errors);

        return Ok(MapToResponse(result.Data!));
    }

    [HttpGet("by-order/{orderId:guid}")]
    [Authorize(Policy = ReadPolicy)]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipmentByOrderId(
        Guid orderId,
        CancellationToken ct)
    {
        var query = new GetShipmentByOrderIdQuery(orderId);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return NotFound(result.Errors);

        return Ok(MapToResponse(result.Data!));
    }

    [HttpGet]
    [Authorize(Policy = ReadPolicy)]
    [ProducesResponseType(typeof(IReadOnlyList<ShipmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyShipments(CancellationToken ct)
    {
        var customerId = _currentUser.UserId;
        if (customerId is null)
            return Unauthorized();

        var query = new GetCustomerShipmentsQuery(customerId.Value);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var response = result.Data!.Select(MapToResponse).ToList();
        return Ok(response);
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