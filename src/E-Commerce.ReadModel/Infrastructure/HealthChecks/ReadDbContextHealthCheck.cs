using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using E_Commerce.ReadModel.DbContext;

namespace E_Commerce.ReadModel.Infrastructure.HealthChecks;

public class ReadDbContextHealthCheck : IHealthCheck
{
    private readonly AppReadDbContext _dbContext;
    public ReadDbContextHealthCheck(AppReadDbContext dbContext) => _dbContext = dbContext;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", ct);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Read DbContext health check failed", ex);
        }
    }
}
