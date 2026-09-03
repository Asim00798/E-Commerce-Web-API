using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.UpdateCartItemQuantity;

[AuthorizePermission(OrderingPermissions.Place)]
public sealed record UpdateCartItemQuantityCommand(
    Guid ProductVariantId,
    int NewQuantity) : IRequest<Result>;