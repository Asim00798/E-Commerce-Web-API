using E_Commerce.Application.BoundedContexts.Orders.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Queries.ListOrders;

[AuthorizePermission(OrderingPermissions.Manage)]
public sealed record ListOrdersQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? Status = null,
    Guid? CustomerId = null) : IRequest<Result<PagedList<OrderListDto>>>;