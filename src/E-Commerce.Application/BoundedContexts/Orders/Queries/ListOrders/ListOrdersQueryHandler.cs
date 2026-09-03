using E_Commerce.Application.BoundedContexts.Orders.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Queries.ListOrders;

public sealed class ListOrdersQueryHandler
    : IRequestHandler<ListOrdersQuery, Result<PagedList<OrderListDto>>>
{
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
        var pageSize = query.PageSize > 0 ? query.PageSize : 20;

        var orders = await _orderRepository.GetPagedAsync(pageNumber, pageSize, ct);
        var totalCount = await _orderRepository.GetTotalCountAsync(ct);

        var items = orders.Select(order => new OrderListDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            Total = order.Total.Amount,
            Currency = order.Total.Currency,
            PlacedAtUtc = order.PlacedAtUtc
        }).ToList();

        var pagedList = new PagedList<OrderListDto>(items, totalCount, pageNumber, pageSize);
        return Result<PagedList<OrderListDto>>.Success(pagedList);
    }
}