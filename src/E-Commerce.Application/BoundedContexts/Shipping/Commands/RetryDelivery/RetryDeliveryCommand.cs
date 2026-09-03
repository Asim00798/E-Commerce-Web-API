using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.RetryDelivery;

[AuthorizePermission(ShippingPermissions.Deliver)]
public sealed record RetryDeliveryCommand(Guid ShipmentId) : IRequest<Result>;