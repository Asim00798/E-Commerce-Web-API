using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.ListProducts;

[AuthorizePermission(CatalogPermissions.ViewProducts)]
public sealed record ListProductsQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedList<ProductListDto>>>;