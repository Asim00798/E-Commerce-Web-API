using E_Commerce.Api.DTOs.Orders.Requests;
using E_Commerce.Api.DTOs.Orders.Responses;
using E_Commerce.Application.BoundedContexts.Orders.Commands.AddCartItem;
using E_Commerce.Application.BoundedContexts.Orders.Commands.ClearCart;
using E_Commerce.Application.BoundedContexts.Orders.Commands.CreateCart;
using E_Commerce.Application.BoundedContexts.Orders.Commands.RemoveCartItem;
using E_Commerce.Application.BoundedContexts.Orders.Commands.UpdateCartItemQuantity;
using E_Commerce.Application.BoundedContexts.Orders.Queries.GetCartByCustomerId;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Orders;

[ApiController]
[Route("api/carts")]
[Authorize(Roles = "Customer")]
public sealed class CartsController : BaseApiController
{
    private readonly ICurrentUser _currentUser;

    public CartsController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// Creates a new cart for the current customer.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCart(CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateCartCommand(), ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(result.Data);
    }

    /// <summary>
    /// Gets the current customer's cart with items.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyCart(CancellationToken ct)
    {
        var query = new GetCartByCustomerIdQuery(_currentUser.UserId!.Value);
        var result = await Mediator.Send(query, ct);

        if (!result.Succeeded)
            return NotFound(result.Errors);

        return Ok(MapToCartResponse(result.Data!));
    }

    /// <summary>
    /// Adds an item to the current customer's cart.
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddCartItem([FromBody] AddCartItemRequest request, CancellationToken ct)
    {
        var unitPrice = new Money(request.UnitPriceAmount, request.UnitPriceCurrency);

        var command = new AddCartItemCommand(
            request.ProductId,
            request.ProductVariantId,
            request.Sku,
            request.ProductName,
            request.VariantName,
            unitPrice,
            request.Quantity);

        var result = await Mediator.Send(command, ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    /// <summary>
    /// Updates the quantity of an item in the cart.
    /// </summary>
    [HttpPut("items/{productVariantId}")]
    public async Task<IActionResult> UpdateCartItemQuantity(
        Guid productVariantId,
        [FromBody] UpdateCartItemQuantityRequest request,
        CancellationToken ct)
    {
        var command = new UpdateCartItemQuantityCommand(productVariantId, request.NewQuantity);
        var result = await Mediator.Send(command, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    [HttpDelete("items/{productVariantId}")]
    public async Task<IActionResult> RemoveCartItem(Guid productVariantId, CancellationToken ct)
    {
        var result = await Mediator.Send(new RemoveCartItemCommand(productVariantId), ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    /// <summary>
    /// Clears all items from the current customer's cart.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        var result = await Mediator.Send(new ClearCartCommand(), ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    private static CartResponse MapToCartResponse(Application.BoundedContexts.Orders.Dtos.CartDto cartDto)
    {
        return new CartResponse
        {
            Id = cartDto.Id,
            CustomerId = cartDto.CustomerId,
            Items = cartDto.Items.Select(i => new CartItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductVariantId = i.ProductVariantId,
                Sku = i.Sku,
                ProductName = i.ProductName,
                VariantName = i.VariantName,
                UnitPrice = i.UnitPrice,
                Currency = i.Currency,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}