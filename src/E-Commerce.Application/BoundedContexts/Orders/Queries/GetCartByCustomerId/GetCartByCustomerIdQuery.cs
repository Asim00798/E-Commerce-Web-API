using E_Commerce.Application.BoundedContexts.Orders.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Queries.GetCartByCustomerId;

[AuthorizePermission(OrderingPermissions.Read)]
public sealed record GetCartByCustomerIdQuery(Guid CustomerId) : IRequest<Result<CartDto>>;