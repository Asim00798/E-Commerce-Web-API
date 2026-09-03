using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Orders.Commands.AddCartItem;

[AuthorizePermission(OrderingPermissions.Place)]
public sealed record AddCartItemCommand(
    Guid ProductId,
    Guid ProductVariantId,
    string Sku,
    string ProductName,
    string VariantName,
    Money UnitPrice,
    int Quantity) : IRequest<Result>;