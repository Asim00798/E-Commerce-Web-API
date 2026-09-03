using E_Commerce.Application.Shared.Files.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.UpdateBrand;

[AuthorizePermission(CatalogPermissions.ManageBrands)]
public sealed record UpdateBrandCommand(
    Guid BrandId,
    string? Name,
    string? DescriptionText,
    FileUpload? NewLogo = null) : IRequest<Result>;