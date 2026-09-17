using E_Commerce.Application.BoundedContexts.Orders.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Queries.ListOrders;

public sealed class ListOrdersQueryHandler
    : IRequestHandler<ListOrdersQuery, Result<PagedList<OrderListDto>>>
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    private readonly IOrderRepository _orderRepository;

    public ListOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<PagedList<OrderListDto>>> Handle(
        ListOrdersQuery query,
        CancellationToken ct)
    {
        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize is > 0 and <= MaxPageSize
            ? query.PageSize
            : DefaultPageSize;

        var orders = await _orderRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            query.Status,
            query.CustomerId,
            ct);

        var totalCount = await _orderRepository.GetTotalCountAsync(
            query.Status,
            query.CustomerId,
            ct);

        var items = orders.Select(order => new OrderListDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            Total = order.Total.Amount,
            Currency = order.Total.Currency,
            PlacedAtUtc = order.PlacedAtUtc
        }).ToList();

        var pagedList = new PagedList<OrderListDto>(
            items,
            totalCount,
            pageNumber,
            pageSize);

        return Result<PagedList<OrderListDto>>.Success(pagedList);
    }
}