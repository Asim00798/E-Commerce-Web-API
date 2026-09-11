using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace E_Commerce.Infrastructure.Extensions;

public static class RepositoryRegistrationExtensions
{
    /// <summary>
    ///     Registers all repository classes and the generic repository in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the repositories to.</param>
    /// <param name="infrastructureAssembly">The assembly containing the repository implementations.</param>
    /// <returns>The updated service collection.</returns>
    
    public static IServiceCollection AddRepositories(
        this IServiceCollection services,
        Assembly infrastructureAssembly)
    {
        // Register all concrete repository classes as their interfaces
        services.Scan(scan => scan
            .FromAssemblies(infrastructureAssembly)
            .AddClasses(classes => classes
                .Where(type => type.Name.EndsWith("Repository") && !type.IsGenericTypeDefinition))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // Register generic repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // 
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}