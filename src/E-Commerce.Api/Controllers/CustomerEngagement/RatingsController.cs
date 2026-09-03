using E_Commerce.Api.DTOs.CustomerEngagement.Requests;
using E_Commerce.Api.DTOs.CustomerEngagement.Responses;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Commands.DeleteRating;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Commands.SubmitRating;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Commands.UpdateRating;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Queries.GetCustomerRatings;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Ratings.Queries.GetProductRatings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.CustomerEngagement;

[ApiController]
[Route("api/engagement/ratings")]
[Authorize(Roles = "Customer")]
public sealed class RatingsController : BaseApiController
{
    /// <summary>
    /// Submits a new rating or updates an existing rating for the authenticated customer.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitRating(
        [FromBody] SubmitRatingRequest request,
        CancellationToken ct)
    {
        var command = new SubmitRatingCommand(
            request.ProductId,
            request.StarRating);

        var result = await Mediator.Send(command, ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { RatingId = result.Data });
    }

    /// <summary>
    /// Updates an existing rating.
    /// </summary>
    [HttpPut("{ratingId:guid}")]
    public async Task<IActionResult> UpdateRating(
        Guid ratingId,
        [FromBody] UpdateRatingRequest request,
        CancellationToken ct)
    {
        var command = new UpdateRatingCommand(
            ratingId,
            request.StarRating);

        var result = await Mediator.Send(command, ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    /// <summary>
    /// Deletes a rating.
    /// </summary>
    [HttpDelete("{ratingId:guid}")]
    public async Task<IActionResult> DeleteRating(
        Guid ratingId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteRatingCommand(ratingId), ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    /// <summary>
    /// Gets the rating summary for a product.
    /// </summary>
    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductRatings(
        Guid productId,
        CancellationToken ct)
    {
        var query = new GetProductRatingsQuery(productId);
        var result = await Mediator.Send(query, ct);

        if (!result.Succeeded)
            return NotFound(result.Errors);

        return Ok(new ProductRatingsSummaryResponse
        {
            ProductId = result.Data!.ProductId,
            AverageRating = result.Data.AverageRating,
            TotalCount = result.Data.TotalCount,
            Distribution = result.Data.Distribution
        });
    }

    /// <summary>
    /// Gets all ratings submitted by the authenticated customer.
    /// </summary>
    [HttpGet("customer")]
    public async Task<IActionResult> GetCustomerRatings(
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCustomerRatingsQuery(), ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var response = result.Data!.Select(r => new RatingResponse
        {
            Id = r.Id,
            ProductId = r.ProductId,
            StarRating = r.StarRating,
            CreatedAtUtc = r.CreatedAtUtc,
            UpdatedAtUtc = r.UpdatedAtUtc
        }).ToList();

        return Ok(response);
    }
}