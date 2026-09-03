using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.AddProductVariant;

[AuthorizePermission(CatalogPermissions.ManageProducts)]
public sealed record AddProductVariantCommand(
    Guid ProductId,
    string Name,
    string? Sku,
    decimal PriceAmount,
    string Currency,
    int StockQuantity) : IRequest<Result<Guid>>;