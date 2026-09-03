using E_Commerce.Application.BoundedContexts.Shipping.Dtos;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Queries.GetCustomerShipments;

[AuthorizePermission(ShippingPermissions.Read)]
public sealed record GetCustomerShipmentsQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<ShipmentDto>>>;