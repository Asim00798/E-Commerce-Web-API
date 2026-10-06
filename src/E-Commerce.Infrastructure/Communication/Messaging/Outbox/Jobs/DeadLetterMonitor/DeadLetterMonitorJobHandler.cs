using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Infrastructure.Communication.Messaging.Outbox.Jobs.DeadLetterMonitor;

/// <summary>
/// Queries the dead-letter table and logs a warning when quarantined messages
/// are present. This is a housekeeping job — it does not publish integration
/// events and does not mutate state.
/// </summary>
public sealed class DeadLetterMonitorJobHandler : IJobHandler<DeadLetterMonitorJobPayload>
{
    private readonly IDeadLetterRepository _deadLetterRepo;
    /* private readonly IAlertService _alertService; */ // replace with your own notification infrastructure
    private readonly ILogger<DeadLetterMonitorJobHandler> _logger;

    public DeadLetterMonitorJobHandler(
        IDeadLetterRepository deadLetterRepo,
        /* IAlertService alertService, */
        ILogger<DeadLetterMonitorJobHandler> logger)
    {
        _deadLetterRepo = deadLetterRepo;
        /* _alertService = alertService; */
        _logger = logger;
    }

    public async Task HandleAsync(
        DeadLetterMonitorJobPayload job,
        CancellationToken cancellationToken)
    {
        var deadMessages = await _deadLetterRepo.GetDeadLetteredAsync(cancellationToken);

        if (deadMessages.Count > 0)
        {
            _logger.LogWarning(
                "Dead-lettered messages detected: {Count}",
                deadMessages.Count);

            /* await _alertService.SendAsync($"Dead-lettered messages: {deadMessages.Count}"); */
        }
    }
}