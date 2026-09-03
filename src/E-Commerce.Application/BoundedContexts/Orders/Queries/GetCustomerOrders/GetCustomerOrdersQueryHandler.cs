using E_Commerce.Application.BoundedContexts.Orders.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Queries.GetCustomerOrders;

public sealed class GetCustomerOrdersQueryHandler
    : IRequestHandler<GetCustomerOrdersQuery, Result<PagedList<OrderListDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionService _permissionService;

    public GetCustomerOrdersQueryHandler(
        IOrderRepository orderRepository,
        ICurrentUser currentUser,
        IPermissionService permissionService)
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    public async Task<Result<PagedList<OrderListDto>>> Handle(
        GetCustomerOrdersQuery query,
        CancellationToken ct)
    {
        // Resource authorization: customer can only access own orders
        bool isOwner = _currentUser.UserId == query.CustomerId;

        if (!isOwner)
            return Result<PagedList<OrderListDto>>.Failure("You are not authorized to view these orders.");

        var allOrders = await _orderRepository.GetByCustomerIdAsync(query.CustomerId, ct);

        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 20;
        var totalCount = allOrders.Count;
        var items = allOrders
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(order => new OrderListDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status.ToString(),
                Total = order.Total.Amount,
                Currency = order.Total.Currency,
                PlacedAtUtc = order.PlacedAtUtc
            })
            .ToList();

        var pagedList = new PagedList<OrderListDto>(items, totalCount, pageNumber, pageSize);
        return Result<PagedList<OrderListDto>>.Success(pagedList);
    }
}