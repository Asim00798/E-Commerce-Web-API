using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProductVariantPrice;

[AuthorizePermission(CatalogPermissions.ManageProducts)]
public sealed record UpdateProductVariantPriceCommand(
    Guid ProductId,
    Guid VariantId,
    decimal NewPriceAmount,
    string Currency) : IRequest<Result>;