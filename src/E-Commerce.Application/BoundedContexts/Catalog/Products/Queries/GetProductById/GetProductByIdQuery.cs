using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.GetProductById;

[AuthorizePermission(CatalogPermissions.ViewProducts)]
public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<Result<ProductDto>>;