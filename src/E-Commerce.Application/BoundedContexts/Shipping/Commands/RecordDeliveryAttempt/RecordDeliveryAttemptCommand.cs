using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using E_Commerce.Domain.BoundedContexts.Core.Shipping.Enums;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Shipping.Commands.RecordDeliveryAttempt;

[AuthorizePermission(ShippingPermissions.Deliver)]
public sealed record RecordDeliveryAttemptCommand(
    Guid ShipmentId,
    DeliveryAttemptResult Result,
    string? FailureReason = null,
    string? Notes = null) : IRequest<Result>;