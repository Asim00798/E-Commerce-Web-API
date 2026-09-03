using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.ListCategories;

[AuthorizePermission(CatalogPermissions.ViewCategories)]
public sealed record ListCategoriesQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedList<CategoryDto>>>;