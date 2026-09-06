using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace E_Commerce.Infrastructure.Observability.HealthChecks;

/// <summary>
/// Verifies Redis connectivity by sending a PING command.
/// A successful PING results in Healthy, regardless of latency.
/// </summary>
public sealed class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;

    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.PingAsync();
            return HealthCheckResult.Healthy("Redis is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis is unreachable.", ex);
        }
    }
}