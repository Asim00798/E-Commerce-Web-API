using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.ReassignDriver;

[AuthorizePermission(ShippingPermissions.Assign)]
public sealed record ReassignDriverCommand(
    Guid ShipmentId,
    Guid NewDriverId) : IRequest<Result>;