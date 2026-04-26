using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.ReadModel.Infrastructure;

/// <summary>
/// Extension methods for registering all ReadModel services, DbContexts, and query handlers
/// into the ASP.NET Core DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all read-side services including DbContexts, handlers, and the query bus.
    /// </summary>
    public static IServiceCollection AddReadModel(this IServiceCollection services)
    {
        // TODO: Register CatalogReadDbContext, query handlers, QueryBus, projection services
        return services;
    }
}
