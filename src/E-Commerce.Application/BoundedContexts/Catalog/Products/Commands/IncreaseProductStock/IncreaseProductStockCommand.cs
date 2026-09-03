using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.IncreaseProductStock;

[AuthorizePermission(CatalogPermissions.ManageProducts)]
public sealed record IncreaseProductStockCommand(
    Guid ProductId,
    Guid VariantId,
    int Quantity) : IRequest<Result>;