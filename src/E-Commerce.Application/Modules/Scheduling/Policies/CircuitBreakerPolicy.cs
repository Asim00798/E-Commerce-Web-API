using E_Commerce.Application.Modules.Scheduling.Abstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.Modules.Scheduling.Policies;

/// <summary>
/// Prevents job execution after a configurable number of consecutive failures.
/// Resets on first success.
/// </summary>
public class CircuitBreakerPolicy : IJobPolicy
{
    private readonly int _failureThreshold;
    private int _failureCount;
    private readonly ILogger<CircuitBreakerPolicy> _logger;

    public CircuitBreakerPolicy(int failureThreshold, ILogger<CircuitBreakerPolicy> logger)
    {
        _failureThreshold = failureThreshold;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync<TJob>(
        TJob job,
        IJobContext context,
        Func<Task> next,
        CancellationToken cancellationToken) where TJob : IJob
    {
        EnsureCircuitClosed(context);
        await ExecuteAndTrackResultAsync(next);
    }

    /// <summary>
    /// Throws an exception if the circuit breaker is open.
    /// </summary>
    private void EnsureCircuitClosed(IJobContext context)
    {
        if (_failureCount >= _failureThreshold)
        {
            _logger.LogWarning("Circuit breaker open – job {JobId} not executed.", context.JobId);
            throw new InvalidOperationException("Circuit breaker is open.");
        }
    }

    /// <summary>
    /// Executes the next delegate and updates the failure counter accordingly.
    /// </summary>
    private async Task ExecuteAndTrackResultAsync(Func<Task> next)
    {
        try
        {
            await next();
            _failureCount = 0; // reset on success
        }
        catch
        {
            _failureCount++;
            throw;
        }
    }
}