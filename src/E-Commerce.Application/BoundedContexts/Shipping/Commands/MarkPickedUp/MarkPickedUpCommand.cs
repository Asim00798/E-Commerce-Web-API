using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.MarkPickedUp;

[AuthorizePermission(ShippingPermissions.Deliver)]
public sealed record MarkPickedUpCommand(Guid ShipmentId) : IRequest<Result>;