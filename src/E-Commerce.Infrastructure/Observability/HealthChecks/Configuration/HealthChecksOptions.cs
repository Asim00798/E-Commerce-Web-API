namespace E_Commerce.Infrastructure.Observability.HealthChecks.Configuration;

/// <summary>
/// Configuration options for health checks.
/// </summary>
public sealed class HealthChecksOptions
{
    /// <summary>
    /// If true, the Redis health check is registered and executed.
    /// </summary>
    public bool RedisEnabled { get; set; } = false;

    /// <summary>
    /// Outbox pending message count that triggers a Degraded status.
    /// </summary>
    public int OutboxWarningThreshold { get; set; } = 100;

    /// <summary>
    /// Outbox pending message count that triggers an Unhealthy status.
    /// </summary>
    public int OutboxErrorThreshold { get; set; } = 500;
}