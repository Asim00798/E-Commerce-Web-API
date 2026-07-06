using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Contracts;
using E_Commerce.Infrastructure.Observability.Abstractions;

namespace E_Commerce.Infrastructure.Scheduling.Hangfire
{
    /// <summary>
    /// Hangfire recurring job that monitors dead‑lettered messages and sends alerts.
    /// </summary>
    public class DeadLetterMonitorJob
    {
        private readonly IDeadLetterRepository _deadLetterRepo;
        private readonly IAlertService _alertService;   // replace with your own notification infrastructure
        private readonly ILogger<DeadLetterMonitorJob> _logger;

        public DeadLetterMonitorJob(
            IDeadLetterRepository deadLetterRepo,
            IAlertService alertService,
            ILogger<DeadLetterMonitorJob> logger)
        {
            _deadLetterRepo = deadLetterRepo;
            _alertService = alertService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var deadMessages = await _deadLetterRepo.GetDeadLetteredAsync(CancellationToken.None);

            if (deadMessages.Count > 0)
            {
                _logger.LogWarning("Dead‑lettered messages detected: {Count}", deadMessages.Count);
                await _alertService.SendAsync($"Dead‑lettered messages: {deadMessages.Count}");
            }
        }
    }
}