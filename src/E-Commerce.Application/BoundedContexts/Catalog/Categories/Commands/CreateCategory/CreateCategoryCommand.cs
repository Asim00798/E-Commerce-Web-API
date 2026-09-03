using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.CreateCategory;

[AuthorizePermission(CatalogPermissions.ManageCategories)]
public sealed record CreateCategoryCommand(
    string Name,
    string Description,
    Guid? ParentCategoryId = null) : IRequest<Result<Guid>>;