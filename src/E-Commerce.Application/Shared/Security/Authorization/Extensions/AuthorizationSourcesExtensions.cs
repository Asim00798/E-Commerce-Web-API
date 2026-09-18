using E_Commerce.Application.Shared.Security.Authorization.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace E_Commerce.Application.Shared.Security.Authorization.Extensions;

/// <summary>
/// Registers the authorization declaration sources discovered by Scrutor.
///
/// Two categories of declarations are registered:
///   - IPermissionSource implementations: one per bounded context, declaring
///     the permission constants that exist in the system.
///   - IRole implementations: one per system role, declaring the role's name
///     and permission set.
///
/// Both are stateless descriptors with no scoped state — Singleton lifetime.
/// They are consumed only by the startup seeders in Infrastructure.
/// </summary>
public static class AuthorizationSourcesExtensions
{
    public static IServiceCollection AddAuthorizationSources(
        this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(c => c.AssignableTo<IPermissionSource>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())
            .AddClasses(c => c.AssignableTo<IRole>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        return services;
    }
}