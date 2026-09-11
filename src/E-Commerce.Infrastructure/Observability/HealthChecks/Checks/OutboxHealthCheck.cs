using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Entities;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace E_Commerce.Infrastructure.Observability.HealthChecks.Checks;

/// <summary>
/// Verifies that the Outbox message store is accessible and queryable.
/// Uses a lightweight existence query separate from the backlog count.
/// </summary>
public sealed class OutboxHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public OutboxHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            // Lightweight query that confirms the Outbox table is reachable.
            await _dbContext.Set<OutboxMessage>().AnyAsync(ct);

            return HealthCheckResult.Healthy(
                "Outbox message store is accessible.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Outbox message store is not accessible.",
                ex);
        }
    }
}

