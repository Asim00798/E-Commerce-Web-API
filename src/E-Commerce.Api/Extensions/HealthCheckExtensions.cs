namespace E_Commerce.Api.Extensions;

public static class HealthCheckExtensions
{
    public static void AddHealthCheckExtension(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<HealthChecks.DatabaseHealthCheck>("Database")
            .AddCheck<HealthChecks.ExternalServiceHealthCheck>("ExternalService");
    }
}
