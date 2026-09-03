using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.ListBrands;

[AuthorizePermission(CatalogPermissions.ViewBrands)]
public sealed record ListBrandsQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedList<BrandDto>>>;