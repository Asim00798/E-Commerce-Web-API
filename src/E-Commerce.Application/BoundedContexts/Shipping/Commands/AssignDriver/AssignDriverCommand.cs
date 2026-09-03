using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.AssignDriver;

[AuthorizePermission(ShippingPermissions.Assign)]
public sealed record AssignDriverCommand(
    Guid ShipmentId,
    Guid DriverId) : IRequest<Result>;