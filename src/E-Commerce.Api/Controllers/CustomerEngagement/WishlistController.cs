using E_Commerce.Api.DTOs.CustomerEngagement.Requests;
using E_Commerce.Api.DTOs.CustomerEngagement.Responses;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.Commands.AddWishlistItem;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.Commands.RemoveWishlistItem;
using E_Commerce.Application.BoundedContexts.CustomerEngagement.Wishlist.Queries.GetCustomerWishlist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.CustomerEngagement;

[ApiController]
[Route("api/engagement/wishlist")]
[Authorize(Roles = "Customer")]
public sealed class WishlistController : BaseApiController
{
    /// <summary>
    /// Gets the authenticated customer's wishlist. If none exists, returns an empty list.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWishlist(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCustomerWishlistQuery(), ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var wishlist = result.Data!;
        return Ok(new WishlistResponse
        {
            Id = wishlist.Id,
            CustomerId = wishlist.CustomerId,
            Items = wishlist.Items.Select(i => new WishlistItemResponse
            {
                ProductId = i.ProductId,
                AddedAtUtc = i.AddedAtUtc
            }).ToList()
        });
    }

    /// <summary>
    /// Adds a product to the authenticated customer's wishlist (auto-creates wishlist if needed).
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddWishlistItemRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new AddWishlistItemCommand(request.ProductId), ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    /// <summary>
    /// Removes a product from the authenticated customer's wishlist.
    /// If the product is not present, returns success (idempotent).
    /// </summary>
    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(
        Guid productId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new RemoveWishlistItemCommand(productId), ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }
}