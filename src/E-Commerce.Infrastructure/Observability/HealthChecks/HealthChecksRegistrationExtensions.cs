
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Observability.HealthChecks;

public static class HealthChecksRegistrationExtensions
{
    /// <summary>
    /// Registers all health checks and their configuration options, including validation.
    /// </summary>
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksSection = configuration.GetSection("HealthChecks");

        services.Configure<HealthChecksOptions>(healthChecksSection);

        services.AddSingleton<
            IValidateOptions<HealthChecksOptions>,
            HealthChecksOptionsValidator>();

        // Read configuration immediately because Redis registration
        // itself is conditional.
        var healthOptions = healthChecksSection.Get<HealthChecksOptions>()
            ?? new HealthChecksOptions();

        var healthCheckBuilder = services.AddHealthChecks();

        healthCheckBuilder
            .AddCheck<SqlServerHealthCheck>("sqlserver")
            .AddCheck<HangfireHealthCheck>("hangfire")
            .AddCheck<OutboxHealthCheck>("outbox")
            .AddCheck<OutboxBacklogHealthCheck>("outbox_backlog");

        if (healthOptions.RedisEnabled)
        {
            healthCheckBuilder.AddCheck<RedisHealthCheck>("redis");
        }

        return services;
    }
}

