using E_Commerce.Application.Shared.Files.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.CreateBrand;

[AuthorizePermission(CatalogPermissions.ManageBrands)]
public sealed record CreateBrandCommand(
    string Name,
    string DescriptionText,
    FileUpload Logo) : IRequest<Result<Guid>>;