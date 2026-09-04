using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProductStock;

[AuthorizePermission(CatalogPermissions.ManageProducts)]
public sealed record UpdateProductStockCommand(
    Guid ProductId,
    Guid ProductVariantId,
    int NewStockQuantity) : IRequest<Result>;