using System.Reflection;
using E_Commerce.Application.Shared.Exceptions;
using E_Commerce.Application.Shared.Security.Authorization.Attributes;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using E_Commerce.Application.Shared.Security.Identity;
using MediatR;

namespace E_Commerce.Application.Shared.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces permission‑based authorization on commands
/// decorated with one or more <see cref="AuthorizePermissionAttribute"/>.
/// </summary>
public sealed class PermissionAuthorizationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationBehavior(
        ICurrentUser currentUser,
        IPermissionService permissionService)
    {
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var attributes = request.GetType().GetCustomAttributes<AuthorizePermissionAttribute>();

        if (attributes.Any())
        {
            var userId = _currentUser.UserId;
            if (userId is null)
                throw new UnauthorizedAccessException();

            foreach (var attribute in attributes)
            {
                var hasPermission = await _permissionService.HasPermissionAsync(
                    userId.Value,
                    attribute.Permission,
                    cancellationToken);

                if (!hasPermission)
                    throw new ForbiddenAccessException();
            }
        }

        return await next();
    }
}