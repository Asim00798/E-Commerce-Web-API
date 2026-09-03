using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.PlaceOrder;

[AuthorizePermission(OrderingPermissions.Place)]
public sealed record PlaceOrderCommand : IRequest<Result<Guid>>;