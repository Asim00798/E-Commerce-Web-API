using E_Commerce.Application.BoundedContexts.Orders.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler
    : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionService _permissionService;

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository,
        ICurrentUser currentUser,
        IPermissionService permissionService)
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    public async Task<Result<OrderDto>> Handle(
        GetOrderByIdQuery query,
        CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(query.OrderId, ct);
        if (order is null)
            return Result<OrderDto>.Failure("Order not found.");

        // Resource authorization: customer can only access own orders
        bool isOwner = _currentUser.UserId == order.CustomerId;
        if (!isOwner)
            return Result<OrderDto>.Failure("You are not authorized to view this order.");

        var orderDto = new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            Subtotal = order.Subtotal.Amount,
            ShippingFee = order.ShippingFee.Amount,
            Total = order.Total.Amount,
            Currency = order.Total.Currency,
            PlacedAtUtc = order.PlacedAtUtc,
            CancelledAtUtc = order.CancelledAtUtc,
            DeliveredAtUtc = order.DeliveredAtUtc,
            RefundedAtUtc = order.RefundedAtUtc,
            Items = order.Items.Select(item => new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                Sku = item.Sku,
                ProductName = item.ProductName,
                VariantName = item.VariantName,
                UnitPrice = item.UnitPrice.Amount,
                Currency = item.UnitPrice.Currency,
                Quantity = item.Quantity,
                LineTotal = item.LineTotal.Amount
            }).ToList()
        };

        return Result<OrderDto>.Success(orderDto);
    }
}