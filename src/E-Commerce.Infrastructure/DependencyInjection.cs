namespace E_Commerce.Infrastructure;

/// <summary>
/// Entry point for registering all Infrastructure services into the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers DbContexts, repositories, services, caching, security, messaging,
    /// health checks, and background jobs.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: Wire up all infrastructure registrations
        return services;
    }
}
