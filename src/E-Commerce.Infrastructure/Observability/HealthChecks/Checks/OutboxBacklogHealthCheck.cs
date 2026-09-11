using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;
using E_Commerce.Infrastructure.Observability.HealthChecks.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Observability.HealthChecks.Checks;

/// <summary>
/// Monitors the number of pending Outbox messages and reports a degraded or unhealthy
/// status when the backlog exceeds configured thresholds.
/// </summary>
public sealed class OutboxBacklogHealthCheck : IHealthCheck
{
    private readonly IOutboxMessageRepository _outboxRepository;
    private readonly HealthChecksOptions _options;

    public OutboxBacklogHealthCheck(
        IOutboxMessageRepository outboxRepository,
        IOptions<HealthChecksOptions> options)
    {
        _outboxRepository = outboxRepository;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            var pendingMessages = await _outboxRepository.GetPendingMessagesAsync(
                _options.OutboxErrorThreshold,
                ct);

            var pending = pendingMessages.Count;

            if (pending >= _options.OutboxErrorThreshold)
            {
                return HealthCheckResult.Unhealthy(
                    $"Outbox backlog is critical: {pending} or more pending messages.");
            }

            if (pending >= _options.OutboxWarningThreshold)
            {
                return HealthCheckResult.Degraded(
                    $"Outbox backlog is elevated: {pending} pending messages.");
            }

            return HealthCheckResult.Healthy(
                $"Outbox backlog is normal: {pending} pending messages.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Unable to determine Outbox backlog.",
                ex);
        }
    }
}

