using E_Commerce.Application.BoundedContexts.Orders.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Queries.GetOrderById;

[AuthorizePermission(OrderingPermissions.Read)]
public sealed record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderDto>>;