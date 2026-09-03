using E_Commerce.Api.DTOs.Orders.Responses;
using E_Commerce.Application.BoundedContexts.Orders.Commands.CancelOrder;
using E_Commerce.Application.BoundedContexts.Orders.Commands.PlaceOrder;
using E_Commerce.Application.BoundedContexts.Orders.Queries.GetCustomerOrders;
using E_Commerce.Application.BoundedContexts.Orders.Queries.GetOrderById;
using E_Commerce.Application.BoundedContexts.Orders.Queries.ListOrders;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Api.Controllers.Orders;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : BaseApiController
{
    private readonly ICurrentUser _currentUser;

    public OrdersController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// Places an order for the current customer.
    /// </summary>
    [HttpPost("place")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> PlaceOrder(CancellationToken ct)
    {
        var result = await Mediator.Send(new PlaceOrderCommand(), ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { OrderId = result.Data });
    }

    /// <summary>
    /// Gets an order by ID (customer, admin, or support).
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Customer, Administrator, Support")]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOrderByIdQuery(id), ct);
        if (!result.Succeeded)
            return NotFound(result.Errors);

        return Ok(MapToOrderResponse(result.Data!));
    }

    /// <summary>
    /// Gets the current customer's orders (paginated).
    /// </summary>
    [HttpGet("customer")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetCustomerOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var query = new GetCustomerOrdersQuery(_currentUser.UserId!.Value, pageNumber, pageSize);
        var result = await Mediator.Send(query, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(MapToPagedOrderList(result.Data!));
    }

    /// <summary>
    /// Lists all orders (admin/support) with optional filters.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Administrator, Support")]
    public async Task<IActionResult> ListOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] Guid? customerId = null,
        CancellationToken ct = default)
    {
        var query = new ListOrdersQuery(pageNumber, pageSize, status, customerId);
        var result = await Mediator.Send(query, ct);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(MapToPagedOrderList(result.Data!));
    }

    /// <summary>
    /// Cancels an order (customer or admin/support).
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Customer, Administrator, Support")]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CancelOrderCommand(id), ct);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    private static OrderResponse MapToOrderResponse(Application.BoundedContexts.Orders.Dtos.OrderDto orderDto)
    {
        return new OrderResponse
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
    }

    private static object MapToPagedOrderList(PagedList<Application.BoundedContexts.Orders.Dtos.OrderListDto> pagedList)
    {
        return new
        {
            Items = pagedList.Items.Select(o => new OrderListResponse
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                Status = o.Status,
                Total = o.Total,
                Currency = o.Currency,
                PlacedAtUtc = o.PlacedAtUtc
            }),
            pagedList.PageNumber,
            pagedList.PageSize,
            pagedList.TotalPages,
            pagedList.TotalCount
        };
    }
}