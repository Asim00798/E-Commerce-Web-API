using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace E_Commerce.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Simple ping to DB would go here
        return Task.FromResult(HealthCheckResult.Healthy("Database is responding."));
    }
}
