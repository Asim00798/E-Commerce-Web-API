using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Profiles.CustomerProfile.Responses;
using E_Commerce.Application.Modules.Profiles.CustomerProfile.Models;
using E_Commerce.Application.Modules.Profiles.CustomerProfile.Queries.GetCustomerProfile;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.v1.Profiles;

/// <summary>
/// Exposes aggregated customer profile views.
/// Self-service for the authenticated user's own profile; administrator lookup for any customer.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/profiles/customer")]
public sealed class CustomerProfileController : BaseApiController
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public CustomerProfileController(ISender sender, ICurrentUser currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    [HttpGet("me")]
    [Authorize(Roles = SystemRoles.Customer)]
    [ProducesResponseType(typeof(CustomerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var customerId = _currentUser.UserId;
        if (customerId is null)
            return Unauthorized();

        var result = await _sender.Send(new GetCustomerProfileQuery(customerId.Value), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(MapToResponse(result.Data!));
    }

    [HttpGet("{customerId:guid}")]
    [Authorize(Roles = SystemRoles.Administrator)]
    [ProducesResponseType(typeof(CustomerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProfileByCustomerId(
        Guid customerId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetCustomerProfileQuery(customerId), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(MapToResponse(result.Data!));
    }

    // ------------------------------------------------------------------
    // Mapping
    // ------------------------------------------------------------------

    private static CustomerProfileResponse MapToResponse(CustomerProfileReadModel model) => new()
    {
        CustomerId = model.CustomerId,
        FullName = model.FullName,
        Email = model.Email,
        PhoneNumber = model.PhoneNumber,
        TotalOrders = model.TotalOrders,
        TotalSpent = model.TotalSpent,
        AverageRating = model.AverageRating,
        WishlistItemCount = model.WishlistItemCount,
        LastOrderDate = model.LastOrderDate
    };

    // ------------------------------------------------------------------
    // Error helpers
    // ------------------------------------------------------------------

    private IActionResult ToNotFoundProblem(string[] errors) =>
        Problem(
            title: "Resource not found.",
            detail: string.Join(" ", errors),
            statusCode: StatusCodes.Status404NotFound);
}