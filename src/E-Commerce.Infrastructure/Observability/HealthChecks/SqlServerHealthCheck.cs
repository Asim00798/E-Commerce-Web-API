using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace E_Commerce.Infrastructure.Observability.HealthChecks;

/// <summary>
/// Verifies SQL Server connectivity using EF Core's CanConnectAsync.
/// </summary>
public sealed class SqlServerHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public SqlServerHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(ct);
            return canConnect
                ? HealthCheckResult.Healthy("SQL Server is reachable.")
                : HealthCheckResult.Unhealthy("SQL Server is unreachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQL Server is unreachable.", ex);
        }
    }
}