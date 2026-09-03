using E_Commerce.Application.Shared.Files.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.AddCategoryImage;

[AuthorizePermission(CatalogPermissions.ManageCategories)]
public sealed record AddCategoryImageCommand(
    Guid CategoryId,
    FileUpload Image) : IRequest<Result<Guid>>;