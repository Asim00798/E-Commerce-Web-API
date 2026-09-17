using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.People.Requests;
using E_Commerce.Api.DTOs.v1.People.Responses;
using E_Commerce.Application.BoundedContexts.People.Commands.CreateMyPerson;
using E_Commerce.Application.BoundedContexts.People.Commands.DeleteMyPerson;
using E_Commerce.Application.BoundedContexts.People.Commands.SetMyPersonalImage;
using E_Commerce.Application.BoundedContexts.People.Commands.UpdateMyPerson;
using E_Commerce.Application.BoundedContexts.People.DTOs;
using E_Commerce.Application.BoundedContexts.People.Queries.GetMyPerson;
using E_Commerce.Application.Shared.Files.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.People;

/// <summary>
/// Manages the current user's Person profile.
/// Self-service only — the Person is resolved from the authenticated user's
/// identity in the Application layer, never from a route parameter, so no
/// other user's Person is addressable here. Fine-grained checks
/// (People.*Own) are enforced via [AuthorizePermission] on the commands and queries.
/// </summary>
[Authorize]
public sealed class PeopleController : BaseApiController
{
    private readonly ISender _sender;

    public PeopleController(ISender sender)
    {
        _sender = sender;
    }

    // ------------------------------------------------------------------
    // Commands
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates the current user's Person profile.
    /// A user may have at most one active Person; a second call returns 400.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PersonResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePersonRequest request,
        CancellationToken ct)
    {
        var command = new CreateMyPersonCommand(
            FirstName: request.FirstName,
            SecondName: request.SecondName,
            ThirdName: request.ThirdName,
            LastName: request.LastName,
            DateOfBirth: request.DateOfBirth!.Value,
            Gender: request.Gender!.Value,
            PhoneNumber: request.PhoneNumber,
            Email: request.Email,
            HomeAddress: ToAddressDto(request.HomeAddress));

        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        // The command returns only the new Person ID. The remainder of the
        // response is assembled from the request, which reflects the persisted
        // state for every field the command accepts (PersonalImageFileId is
        // null at creation — it is set by a separate endpoint).
        var response = new PersonResponse(
            Id: result.Data,
            FirstName: request.FirstName,
            SecondName: request.SecondName,
            ThirdName: request.ThirdName,
            LastName: request.LastName,
            DateOfBirth: request.DateOfBirth!.Value,
            Gender: request.Gender!.Value.ToString(),
            PhoneNumber: request.PhoneNumber,
            Email: request.Email,
            HomeAddress: ToAddressResponse(request.HomeAddress),
            PersonalImageFileId: null);

        return CreatedAtAction(nameof(GetMe), null, response);
    }

    /// <summary>
    /// Updates phone number, email, and home address on the current user's
    /// Person. Name, gender, and date of birth are immutable after creation.
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdatePersonRequest request,
        CancellationToken ct)
    {
        var command = new UpdateMyPersonCommand(
            PhoneNumber: request.PhoneNumber,
            Email: request.Email,
            HomeAddress: ToAddressDto(request.HomeAddress));

        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return NoContent();
    }

    /// <summary>
    /// Soft-deletes the current user's Person. The Identity account is
    /// untouched — the user can create a new Person later.
    /// </summary>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMe(CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteMyPersonCommand(), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return NoContent();
    }

    /// <summary>
    /// Uploads and sets the current user's personal image, replacing any
    /// existing one.
    /// </summary>
    [HttpPut("me/image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetMyImage(
        [FromForm] SetPersonalImageRequest request,
        CancellationToken ct)
    {
        var upload = new FileUpload(
            request.Image.OpenReadStream(),
            request.Image.FileName,
            request.Image.ContentType);

        var result = await _sender.Send(new SetMyPersonalImageCommand(upload), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return NoContent();
    }

    // ------------------------------------------------------------------
    // Queries
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns the current user's Person profile.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(PersonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await _sender.Send(new GetMyPersonQuery(), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(MapToResponse(result.Data!));
    }

    // ------------------------------------------------------------------
    // Mapping — Application DTO → API DTO
    // ------------------------------------------------------------------

    private static AddressDto ToAddressDto(AddressRequest request) => new()
    {
        Street = request.Street,
        City = request.City,
        Type = request.Type,
        LocationMapUrl = request.LocationMapUrl
    };

    private static AddressResponse ToAddressResponse(AddressRequest request) => new(
        Street: request.Street,
        City: request.City,
        Type: request.Type.ToString(),
        LocationMapUrl: request.LocationMapUrl);

    private static PersonResponse MapToResponse(PersonDto dto) => new(
        Id: dto.Id,
        FirstName: dto.FirstName,
        SecondName: dto.SecondName,
        ThirdName: dto.ThirdName,
        LastName: dto.LastName,
        DateOfBirth: dto.DateOfBirth,
        Gender: dto.Gender,
        PhoneNumber: dto.PhoneNumber,
        Email: dto.Email,
        HomeAddress: dto.HomeAddress is null
            ? null
            : new AddressResponse(
                Street: dto.HomeAddress.Street,
                City: dto.HomeAddress.City,
                Type: dto.HomeAddress.Type.ToString(),
                LocationMapUrl: dto.HomeAddress.LocationMapUrl),
        PersonalImageFileId: dto.PersonalImageFileId);

    // ------------------------------------------------------------------
    // Error helpers — shape matches GlobalExceptionMiddleware output
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