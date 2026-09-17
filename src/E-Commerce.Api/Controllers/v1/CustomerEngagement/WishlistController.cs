using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.CustomerEngagement.Requests;
using E_Commerce.Api.DTOs.v1.CustomerEngagement.Responses;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.Commands.AddWishlistItem;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.Commands.RemoveWishlistItem;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.Queries.GetCustomerWishlist;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.CustomerEngagement;

/// <summary>
/// Manages the authenticated customer's wishlist.
/// All actions require the Customer role.
/// The wishlist is auto-created on first item add; there is no explicit create endpoint.
/// </summary>
[ApiController]
[Route("api/engagement/wishlist")]
[Authorize(Roles = SystemRoles.Customer)]
public sealed class WishlistController : BaseApiController
{
    private readonly ISender _sender;

    public WishlistController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Gets the authenticated customer's wishlist.
    /// Returns an empty wishlist if none exists (does not 404).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WishlistResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWishlist(CancellationToken ct)
    {
        var result = await _sender.Send(new GetCustomerWishlistQuery(), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        var wishlist = result.Data!;

        return Ok(new WishlistResponse
        {
            Id = wishlist.Id,
            CustomerId = wishlist.CustomerId,
            Items = wishlist.Items
                .Select(i => new WishlistItemResponse
                {
                    ProductId = i.ProductId,
                    AddedAtUtc = i.AddedAtUtc
                })
                .ToList()
        });
    }

    /// <summary>
    /// Adds a product to the authenticated customer's wishlist.
    /// Auto-creates the wishlist if it does not exist. Idempotent: adding an
    /// already-present product is a no-op and returns the same success result.
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddItem(
        [FromBody] AddWishlistItemRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new AddWishlistItemCommand(request.ProductId), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return NoContent();
    }

    /// <summary>
    /// Removes a product from the authenticated customer's wishlist.
    /// Idempotent: removing a product not present is treated as success.
    /// </summary>
    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveItem(
        Guid productId,
        CancellationToken ct)
    {
        var result = await _sender.Send(
            new RemoveWishlistItemCommand(productId), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return NoContent();
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
}