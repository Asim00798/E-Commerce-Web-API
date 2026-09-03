using E_Commerce.Application.BoundedContexts.Orders.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Queries.GetCustomerOrders;

[AuthorizePermission(OrderingPermissions.Read)]
public sealed record GetCustomerOrdersQuery(
    Guid CustomerId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedList<OrderListDto>>>;