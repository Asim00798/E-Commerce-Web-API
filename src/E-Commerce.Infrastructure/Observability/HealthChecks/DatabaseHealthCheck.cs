using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace E_Commerce.Infrastructure.Observability.HealthChecks;

/// <summary>
/// Health check that verifies connectivity to the primary SQL database.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    // TODO: Inject a DbContext or raw IDbConnection to probe the database

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // TODO: Execute a lightweight query (e.g., SELECT 1) and return Healthy / Unhealthy
        throw new NotImplementedException();
    }
}
