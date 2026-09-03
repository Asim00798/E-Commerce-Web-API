using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.RemoveProductVariant;

[AuthorizePermission(CatalogPermissions.ManageProducts)]
public sealed record RemoveProductVariantCommand(
    Guid ProductId,
    Guid VariantId) : IRequest<Result>;