using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Shipping.Responses;
using E_Commerce.Application.BoundedContexts.Shipping.Dtos;
using E_Commerce.Application.BoundedContexts.Shipping.Queries.GetCustomerShipments;
using E_Commerce.Application.BoundedContexts.Shipping.Queries.GetShipmentById;
using E_Commerce.Application.BoundedContexts.Shipping.Queries.GetShipmentByOrderId;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.Shipping;

/// <summary>
/// Customer-facing shipment API. Only allows access to shipments owned by the
/// authenticated customer. Ownership is enforced by the query handlers.
/// </summary>
[ApiController]
[Route("api/customer/shipments")]
[Authorize(Roles = $"{SystemRoles.Customer}")]
public sealed class CustomerShipmentsController : BaseApiController
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public CustomerShipmentsController(ISender sender, ICurrentUser currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Gets a shipment by ID. Only the shipment's owner can access it;
    /// ownership is enforced by the query handler.
    /// </summary>
    [HttpGet("{shipmentId:guid}")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipmentById(
        Guid shipmentId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetShipmentByIdQuery(shipmentId), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(MapToResponse(result.Data!));
    }

    /// <summary>
    /// Gets the shipment associated with an order. Only the order's owner can
    /// access it; ownership is enforced by the query handler.
    /// </summary>
    [HttpGet("by-order/{orderId:guid}")]
    [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipmentByOrderId(
        Guid orderId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetShipmentByOrderIdQuery(orderId), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(MapToResponse(result.Data!));
    }

    /// <summary>
    /// Gets all shipments for the current customer.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ShipmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyShipments(CancellationToken ct)
    {
        var customerId = _currentUser.UserId;

        if (customerId is null)
            return Unauthorized();

        var result = await _sender.Send(
            new GetCustomerShipmentsQuery(customerId.Value), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        var response = result.Data!.Select(MapToResponse).ToList();
        return Ok(response);
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