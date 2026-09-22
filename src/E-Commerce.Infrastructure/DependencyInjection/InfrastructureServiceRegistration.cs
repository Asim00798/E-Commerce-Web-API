using E_Commerce.Infrastructure.Caching.Extensions;
using E_Commerce.Infrastructure.Extensions;
using E_Commerce.Infrastructure.Persistence.Extensions;
using E_Commerce.Infrastructure.Stock.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Infrastructure.DependencyInjection;

/// <summary>
/// Entry point for registering all Infrastructure services into the DI container.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Registers DbContexts, repositories, services, caching, security, messaging,
    /// health checks, and background jobs...etc
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Persistence
        services.AddPersistence(configuration);
        services.AddMigrationOptions(configuration);
        services.AddPersistenceInterceptors();
        // Caching
        services.AddRedisCaching(configuration);

        // Stock
        services.AddStockInfrastructure();

        return services;
    }
}