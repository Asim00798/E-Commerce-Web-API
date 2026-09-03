using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.RemoveCategoryImage;

[AuthorizePermission(CatalogPermissions.ManageCategories)]
public sealed record RemoveCategoryImageCommand(
    Guid CategoryId,
    Guid FileId) : IRequest<Result>;