using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.CreateProduct;

[AuthorizePermission(CatalogPermissions.ManageProducts)]
public sealed record CreateProductCommand(
    string Name,
    string? ShortDescription,
    string? LongDescription,
    string? Dimensions,
    string? Weight,
    DateTimeOffset? DateOfManufacture,
    DateTimeOffset? DateOfExpiry,
    string? Material,
    string? Color,
    Guid BrandId,
    Guid CategoryId,
    List<string>? Tags = null) : IRequest<Result<Guid>>;