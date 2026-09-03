using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.DeletePermission;

[AuthorizeRole("Administrator")]
[AuthorizePermission(AuthorizationPermissions.Manage)]
public sealed record DeletePermissionCommand(Guid PermissionId) : IRequest<Result>;