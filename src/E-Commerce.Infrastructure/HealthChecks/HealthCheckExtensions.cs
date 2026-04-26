using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace E_Commerce.Infrastructure.HealthChecks;

/// <summary>
/// Extension methods for registering and mapping health check endpoints.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Registers all infrastructure health checks (database, Redis, external services).
    /// </summary>
    public static IServiceCollection AddInfrastructureHealthChecks(this IServiceCollection services)
    {
        // TODO: Add DatabaseHealthCheck, Redis check, event bus check, etc.
        return services;
    }

    /// <summary>
    /// Maps the health check endpoint to <c>/health</c>.
    /// </summary>
    public static WebApplication MapInfrastructureHealthChecks(this WebApplication app)
    {
        // TODO: app.MapHealthChecks("/health", new HealthCheckOptions { ... });
        return app;
    }
}
