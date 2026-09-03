using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.SetMainProductImage;

[AuthorizePermission(CatalogPermissions.ManageProducts)]
public sealed record SetMainProductImageCommand(
    Guid ProductId,
    Guid ImageId) : IRequest<Result>;