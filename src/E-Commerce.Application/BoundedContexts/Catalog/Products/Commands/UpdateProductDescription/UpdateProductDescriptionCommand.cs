using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProductDescription;

[AuthorizePermission(CatalogPermissions.ManageProducts)]
public sealed record UpdateProductDescriptionCommand(
    Guid ProductId,
    string Name,
    string? ShortDescription,
    string? LongDescription,
    string? Material,
    string? Color) : IRequest<Result>;