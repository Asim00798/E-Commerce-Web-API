using E_Commerce.Application.BoundedContexts.Shipping.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Queries.GetShipmentById;

[AuthorizePermission(ShippingPermissions.Read)]
public sealed record GetShipmentByIdQuery(Guid ShipmentId) : IRequest<Result<ShipmentDto>>;