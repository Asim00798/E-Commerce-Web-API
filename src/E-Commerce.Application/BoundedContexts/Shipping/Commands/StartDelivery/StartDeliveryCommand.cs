using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.StartDelivery;

[AuthorizePermission(ShippingPermissions.Deliver)]
public sealed record StartDeliveryCommand(Guid ShipmentId) : IRequest<Result>;