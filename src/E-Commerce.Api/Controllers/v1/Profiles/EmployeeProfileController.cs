using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Profiles.EmployeeProfile.Responses;
using E_Commerce.Application.Modules.Profiles.EmployeeProfile.Models;
using E_Commerce.Application.Modules.Profiles.EmployeeProfile.Queries.GetEmployeeProfile;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.v1.Profiles;

/// <summary>
/// Exposes aggregated employee profile views.
/// Administrator-only.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/profiles/employee")]
public sealed class EmployeeProfileController : BaseApiController
{
    private readonly ISender _sender;

    public EmployeeProfileController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{employeeId:guid}")]
    [Authorize(Roles = SystemRoles.Administrator)]
    [ProducesResponseType(typeof(EmployeeProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProfileByEmployeeId(
        Guid employeeId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetEmployeeProfileQuery(employeeId), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(MapToResponse(result.Data!));
    }

    // ------------------------------------------------------------------
    // Mapping
    // ------------------------------------------------------------------

    private static EmployeeProfileResponse MapToResponse(EmployeeProfileReadModel model) => new()
    {
        EmployeeId = model.EmployeeId,
        FullName = model.FullName,
        Department = model.Department,
        ActiveShipments = model.ActiveShipments,
        CompletedShipments = model.CompletedShipments,
        AverageRating = model.AverageRating,
        LastActiveAt = model.LastActiveAt
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