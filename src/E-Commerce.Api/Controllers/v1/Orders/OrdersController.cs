using E_Commerce.Api.Controllers.Common;
using E_Commerce.Api.DTOs.v1.Orders.Responses;
using E_Commerce.Api.DTOs.v1.Shared;
using E_Commerce.Application.BoundedContexts.Orders.Commands.CancelOrder;
using E_Commerce.Application.BoundedContexts.Orders.Commands.PlaceOrder;
using E_Commerce.Application.BoundedContexts.Orders.Dtos;
using E_Commerce.Application.BoundedContexts.Orders.Queries.GetCustomerOrders;
using E_Commerce.Application.BoundedContexts.Orders.Queries.GetOrderById;
using E_Commerce.Application.BoundedContexts.Orders.Queries.ListOrders;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Roles;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.AggregateRoots.Order.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_Commerce.Api.Controllers.v1.Orders;

/// <summary>
/// Manages customer orders.
/// Customers can place, view, and cancel their own orders.
/// Administrators and Support can view and cancel any order.
/// Resource ownership is enforced by the command and query handlers.
/// </summary>
[ApiController]
[Route("api/orders")]
public sealed class OrdersController : BaseApiController
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public OrdersController(ISender sender, ICurrentUser currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    // ------------------------------------------------------------------
    // Commands
    // ------------------------------------------------------------------

    /// <summary>
    /// Places an order from the current customer's cart.
    /// Returns 201 with a Location header pointing to the created order.
    /// </summary>
    [HttpPost("place")]
    [Authorize(Roles = SystemRoles.Customer)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PlaceOrder(CancellationToken ct)
    {
        var result = await _sender.Send(new PlaceOrderCommand(), ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = result.Data },
            result.Data);
    }

    /// <summary>
    /// Cancels an order. Customers can cancel their own orders; Administrators
    /// and Support can cancel any order.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = $"{SystemRoles.Customer},{SystemRoles.Administrator},{SystemRoles.Support}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new CancelOrderCommand(id), ct);

        return result.Succeeded ? NoContent() : ToValidationProblem(result.Errors);
    }

    // ------------------------------------------------------------------
    // Queries
    // ------------------------------------------------------------------

    /// <summary>
    /// Gets an order by ID. Customers can only access their own orders;
    /// Administrators and Support can access any order.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{SystemRoles.Customer},{SystemRoles.Administrator},{SystemRoles.Support}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetOrderByIdQuery(id), ct);

        if (!result.Succeeded)
            return ToNotFoundProblem(result.Errors);

        return Ok(MapToOrderResponse(result.Data!));
    }

    /// <summary>
    /// Gets the current customer's orders (paginated).
    /// </summary>
    [HttpGet("customer")]
    [Authorize(Roles = SystemRoles.Customer)]
    [ProducesResponseType(typeof(PaginatedResponse<OrderListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetCustomerOrdersQuery(GetCurrentUserId(), pageNumber, pageSize);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return Ok(MapToPagedOrderList(result.Data!));
    }

    /// <summary>
    /// Lists all orders with optional filters. Available to Administrators and Support.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{SystemRoles.Administrator},{SystemRoles.Support}")]
    [ProducesResponseType(typeof(PaginatedResponse<OrderListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] Guid? customerId = null,
        CancellationToken ct = default)
    {
        var query = new ListOrdersQuery(pageNumber, pageSize, status, customerId);
        var result = await _sender.Send(query, ct);

        if (!result.Succeeded)
            return ToValidationProblem(result.Errors);

        return Ok(MapToPagedOrderList(result.Data!));
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private Guid GetCurrentUserId()
    {
        if (_currentUser.UserId is not { } userId)
            throw new UnauthorizedAccessException(
                "Authenticated principal is missing a valid user identifier.");

        return userId;
    }

    private static OrderResponse MapToOrderResponse(OrderDto orderDto) => new()
    {
        Id = orderDto.Id,
        CustomerId = orderDto.CustomerId,
        Status = orderDto.Status,
        Subtotal = orderDto.Subtotal,
        ShippingFee = orderDto.ShippingFee,
        Total = orderDto.Total,
        Currency = orderDto.Currency,
        PlacedAtUtc = orderDto.PlacedAtUtc,
        CancelledAtUtc = orderDto.CancelledAtUtc,
        DeliveredAtUtc = orderDto.DeliveredAtUtc,
        RefundedAtUtc = orderDto.RefundedAtUtc,
        Items = orderDto.Items.Select(i => new OrderItemResponse
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductVariantId = i.ProductVariantId,
            Sku = i.Sku,
            ProductName = i.ProductName,
            VariantName = i.VariantName,
            UnitPrice = i.UnitPrice,
            Currency = i.Currency,
            Quantity = i.Quantity,
            LineTotal = i.LineTotal
        }).ToList()
    };

    private static PaginatedResponse<OrderListResponse> MapToPagedOrderList(
        PagedList<OrderListDto> pagedList) => new()
        {
            Items = pagedList.Items.Select(o => new OrderListResponse
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                Status = o.Status,
                Total = o.Total,
                Currency = o.Currency,
                PlacedAtUtc = o.PlacedAtUtc
            }).ToList(),
            PageNumber = pagedList.PageNumber,
            PageSize = pagedList.PageSize,
            TotalPages = pagedList.TotalPages,
            TotalCount = pagedList.TotalCount,
            HasPreviousPage = pagedList.HasPreviousPage,
            HasNextPage = pagedList.HasNextPage
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