using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.DeleteRole;

[AuthorizeRole("Administrator")]
[AuthorizePermission(AuthorizationPermissions.Manage)]
public sealed record DeleteRoleCommand(Guid RoleId) : IRequest<Result>;