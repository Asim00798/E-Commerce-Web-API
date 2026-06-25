using E_Commerce.Application.Shared.Identity;
using E_Commerce.Infrastructure.Extensions;
using E_Commerce.Infrastructure.Identity;
using E_Commerce.Infrastructure.Identity.Services;
using E_Commerce.Infrastructure.Scheduling.Extensions;

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
        services.AddInfrastructureServices(configuration);
        SchedulingInfrastructureExtensions.AddSchedulingInfrastructure(services, configuration.GetConnectionString("Hangfire"));
        // TODO: Wire up other infrastructure registrations (security, messaging, etc.)
        services.AddScoped<IIdentityService, IdentityService>();
        return services;
    }
}
