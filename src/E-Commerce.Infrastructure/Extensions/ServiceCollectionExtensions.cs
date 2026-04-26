namespace E_Commerce.Infrastructure.Extensions;

/// <summary>
/// IServiceCollection extension methods for registering all Infrastructure services
/// in a single composable call.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers DbContexts, repositories, caching, security, messaging, and external clients.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: Compose all sub-registrations
        return services;
    }
}
