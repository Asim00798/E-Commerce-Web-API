using E_Commerce.Application.Shared.Files.Models;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.People.Commands.SetMyPersonalImage;

[AuthorizePermission(PeoplePermissions.UpdateOwn)]
public sealed record SetMyPersonalImageCommand(
    FileUpload Image) : IRequest<Result>;