using System.Reflection;
using MediatR;
using E_Commerce.Application.Common.Exceptions;
using E_Commerce.Application.BoundedContexts.Security;
using E_Commerce.Application.Shared.Identity;

namespace E_Commerce.Application.Common.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUser currentUser) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (authorizeAttributes.Any())
        {
            if (!currentUser.IsAuthenticated)
            {
                throw new UnauthorizedAccessException();
            }

            var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles));

            if (authorizeAttributesWithRoles.Any())
            {
                var authorized = false;

                foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
                {
                    foreach (var role in roles)
                    {
                        if (currentUser.Roles.Contains(role.Trim()))
                        {
                            authorized = true;
                            break;
                        }
                    }
                    if (authorized) break;
                }

                if (!authorized)
                {
                    throw new ForbiddenAccessException();
                }
            }
        }

        return await next();
    }
}
