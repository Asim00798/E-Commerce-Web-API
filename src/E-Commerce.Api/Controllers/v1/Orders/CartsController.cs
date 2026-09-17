using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Orders.Requests;
using E_Commerce.Api.DTOs.v1.Orders.Responses;
using E_Commerce.Application.BoundedContexts.Orders.Commands.AddCartItem;
using E_Commerce.Application.BoundedContexts.Orders.Commands.ClearCart;
using E_Commerce.Application.BoundedContexts.Orders.Commands.CreateCart;
using E_Commerce.Application.BoundedContexts.Orders.Commands.RemoveCartItem;
using E_Commerce.Application.BoundedContexts.Orders.Commands.UpdateCartItemQuantity;
using E_Commerce.Application.BoundedContexts.Orders.Dtos;
using E_Commerce.Application.BoundedContexts.Orders.Queries.GetCartByCustomerId;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.Orders;

/// <summary>
/// Manages the current customer's shopping cart.
/// All actions operate on the caller's own cart; there is no cross-customer access.
/// </summary>
[ApiController]
[Route("api/carts")]
[Authorize(Roles = SystemRoles.Customer)]
public sealed class CartsController : BaseApiController
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public CartsController(ISender sender, ICurrentUser currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    // ------------------------------------------------------------------
    // Cart
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates a new cart for the current customer.
    /// Returns the new cart identifier.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCart(CancellationToken ct)
    {
        var result = await _sender.Send(new CreateCartCommand(), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Gets the current customer's cart with items.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyCart(CancellationToken ct)
    {
        var result = await _sender.Send(
            new GetCartByCustomerIdQuery(GetCurrentUserId()), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(MapToCartResponse(result.Data!));
    }

    /// <summary>
    /// Clears all items from the current customer's cart.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        var result = await _sender.Send(new ClearCartCommand(), ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    // ------------------------------------------------------------------
    // Cart items
    // ------------------------------------------------------------------

    /// <summary>
    /// Adds an item to the current customer's cart.
    /// </summary>
    /// <remarks>
    /// The request currently carries price and display fields supplied by the
    /// client. These values are stored verbatim on the cart line and become
    /// the order-line snapshot at checkout. Until the Application layer is
    /// changed to fetch authoritative product data from Catalog, clients can
    /// tamper with the price. See the review notes for the required fix.
    /// </remarks>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddCartItem(
        [FromBody] AddCartItemRequest request,
        CancellationToken ct)
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

        var result = await _sender.Send(command, ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    /// <summary>
    /// Updates the quantity of an item in the current customer's cart.
    /// </summary>
    [HttpPut("items/{productVariantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCartItemQuantity(
        Guid productVariantId,
        [FromBody] UpdateCartItemQuantityRequest request,
        CancellationToken ct)
    {
        var command = new UpdateCartItemQuantityCommand(productVariantId, request.NewQuantity);
        var result = await _sender.Send(command, ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    /// <summary>
    /// Removes an item from the current customer's cart.
    /// </summary>
    [HttpDelete("items/{productVariantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveCartItem(
        Guid productVariantId,
        CancellationToken ct)
    {
        var result = await _sender.Send(new RemoveCartItemCommand(productVariantId), ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Resolves the current user's identifier from the authenticated principal.
    /// Throws if the claim is missing or malformed.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedAccessException(
                "Authenticated principal is missing a valid user identifier.");

        return userId;
    }

    private static CartResponse MapToCartResponse(CartDto cartDto) => new()
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