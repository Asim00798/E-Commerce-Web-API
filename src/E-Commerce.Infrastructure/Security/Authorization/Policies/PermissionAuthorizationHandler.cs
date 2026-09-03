using E_Commerce.Application.Shared.Security.Authorization.Services;
using E_Commerce.Application.Shared.Security.Identity;
using Microsoft.AspNetCore.Authorization;

namespace E_Commerce.Infrastructure.Security.Authorization.Policies;

/// <summary>
/// Handles <see cref="PermissionRequirement"/> by checking the current user's permissions
/// through the application-level <see cref="IPermissionService"/>.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(
        ICurrentUser currentUser,
        IPermissionService permissionService)
    {
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return;

        if (await _permissionService.HasPermissionAsync(userId.Value, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}