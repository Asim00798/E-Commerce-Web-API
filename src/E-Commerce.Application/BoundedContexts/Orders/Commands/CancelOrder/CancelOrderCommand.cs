using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.CancelOrder;

[AuthorizePermission(OrderingPermissions.Cancel)]
public sealed record CancelOrderCommand(Guid OrderId) : IRequest<Result>;