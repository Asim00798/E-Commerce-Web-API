using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.UpdateCategory;

[AuthorizePermission(CatalogPermissions.ManageCategories)]
public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    string? Name,
    string? Description,
    Guid? ParentCategoryId = null,
    bool ClearParent = false) : IRequest<Result>;