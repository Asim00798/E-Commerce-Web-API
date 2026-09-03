using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.CreateCart;

[AuthorizePermission(OrderingPermissions.Place)]
public sealed record CreateCartCommand : IRequest<Result<Guid>>;