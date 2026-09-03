using E_Commerce.Application.Shared.Files.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.AddProductImage;

[AuthorizePermission(CatalogPermissions.ManageProducts)]
public sealed record AddProductImageCommand(
    Guid ProductId,
    FileUpload Image) : IRequest<Result<Guid>>;