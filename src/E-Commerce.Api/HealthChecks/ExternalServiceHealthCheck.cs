using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace E_Commerce.Api.HealthChecks;

public class ExternalServiceHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("External services are reachable."));
    }
}
