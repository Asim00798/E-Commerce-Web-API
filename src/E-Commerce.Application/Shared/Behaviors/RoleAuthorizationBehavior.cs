using System.Reflection;
using E_Commerce.Application.Shared.Exceptions;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;

namespace E_Commerce.Application.Shared.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces role‑based authorization on commands
/// decorated with <see cref="AuthorizeRoleAttribute"/>.
/// </summary>
public sealed class RoleAuthorizationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRoleService _userRoleService;

    public RoleAuthorizationBehavior(
        ICurrentUser currentUser,
        IUserRoleService userRoleService)
    {
        _currentUser = currentUser;
        _userRoleService = userRoleService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var attribute = request.GetType().GetCustomAttribute<AuthorizeRoleAttribute>();

        if (attribute is not null)
        {
            var userId = _currentUser.UserId;
            if (userId is null)
                throw new UnauthorizedAccessException();

            var hasRole = await _userRoleService.HasRoleAsync(
                userId.Value,
                attribute.Role,
                cancellationToken);

            if (!hasRole)
                throw new ForbiddenAccessException();
        }

        return await next();
    }
}