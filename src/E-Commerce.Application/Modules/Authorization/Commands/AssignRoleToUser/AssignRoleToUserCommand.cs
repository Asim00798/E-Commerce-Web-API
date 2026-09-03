using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;
using MediatR;

namespace E_Commerce.Application.Modules.Authorization.Commands.AssignRoleToUser;

[AuthorizeRole("Administrator")]
[AuthorizePermission(AuthorizationPermissions.Manage)]
public sealed record AssignRoleToUserCommand(
    Guid UserId,
    string Role) : IRequest<Result>;