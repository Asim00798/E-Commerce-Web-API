using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.GetCategoryById;

[AuthorizePermission(CatalogPermissions.ViewCategories)]
public sealed record GetCategoryByIdQuery(Guid CategoryId) : IRequest<Result<CategoryDto>>;