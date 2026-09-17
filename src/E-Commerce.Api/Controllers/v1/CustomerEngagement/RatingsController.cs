using E_Commerce.Api.Attributes;
using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.CustomerEngagement.Requests;
using E_Commerce.Api.DTOs.v1.CustomerEngagement.Responses;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Commands.DeleteRating;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Commands.SubmitRating;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Commands.UpdateRating;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Queries.GetCustomerRatings;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Queries.GetProductRatings;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.CustomerEngagement;

/// <summary>
/// Manages product ratings for the authenticated customer.
/// The product ratings summary is public; all other actions require the Customer role.
/// </summary>
[ApiController]
[Route("api/engagement/ratings")]
[Authorize(Roles = SystemRoles.Customer)]
public sealed class RatingsController : BaseApiController
{
    private readonly ISender _sender;

    public RatingsController(ISender sender)
    {
        _sender = sender;
    }

    // ------------------------------------------------------------------
    // Customer actions
    // ------------------------------------------------------------------

    /// <summary>
    /// Submits a new rating or updates the existing rating for the authenticated customer.
    /// One rating per customer per product.
    /// </summary>
    /// <remarks>
    /// Response body shape: <c>{ "ratingId": "..." }</c>.
    /// Returns the rating identifier so the client can reference it for
    /// subsequent update or delete operations.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitRating(
        [FromBody] SubmitRatingRequest request,
        CancellationToken ct)
    {
        var command = new SubmitRatingCommand(request.ProductId, request.StarRating);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return Ok(new { RatingId = result.Data });
    }

    /// <summary>
    /// Updates the star value of an existing rating owned by the authenticated customer.
    /// </summary>
    [HttpPut("{ratingId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRating(
        Guid ratingId,
        [FromBody] UpdateRatingRequest request,
        CancellationToken ct)
    {
        var command = new UpdateRatingCommand(ratingId, request.StarRating);
        var result = await _sender.Send(command, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return NoContent();
    }

    /// <summary>
    /// Permanently deletes a rating owned by the authenticated customer.
    /// </summary>
    [HttpDelete("{ratingId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteRating(
        Guid ratingId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteRatingCommand(ratingId), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return NoContent();
    }

    /// <summary>
    /// Gets all ratings submitted by the authenticated customer.
    /// </summary>
    [HttpGet("customer")]
    [ProducesResponseType(typeof(IReadOnlyList<RatingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerRatings(CancellationToken ct)
    {
        var result = await _sender.Send(new GetCustomerRatingsQuery(), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        var response = result.Data!
            .Select(r => new RatingResponse
            {
                Id = r.Id,
                ProductId = r.ProductId,
                StarRating = r.StarRating,
                CreatedAtUtc = r.CreatedAtUtc,
                UpdatedAtUtc = r.UpdatedAtUtc
            })
            .ToList();

        return Ok(response);
    }

    // ------------------------------------------------------------------
    // Public actions
    // ------------------------------------------------------------------

    /// <summary>
    /// Gets the aggregate ratings summary for a product. Available to any caller.
    /// </summary>
    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    [CacheControl(Public = true, MaxAge = 300)]
    [ProducesResponseType(typeof(ProductRatingsSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductRatings(
        Guid productId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new GetProductRatingsQuery(productId), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(new ProductRatingsSummaryResponse
        {
            ProductId = result.Data!.ProductId,
            AverageRating = result.Data.AverageRating,
            TotalCount = result.Data.TotalCount,
            Distribution = result.Data.Distribution
        });
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

    private IActionResult ToNotFoundProblem(string[] errors) =>
        Problem(
            title: "Resource not found.",
            detail: string.Join(" ", errors),
            statusCode: StatusCodes.Status404NotFound);
}