using E_Commerce.Infrastructure.Communication.Notifications.Contracts;
using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using E_Commerce.Infrastructure.Communication.Notifications.Messages;

namespace E_Commerce.Infrastructure.Communication.Notifications.External.Push.Transport;

/// <summary>
/// Decorator around <see cref="IPushTransport"/> that records every push attempt
/// in the <see cref="NotificationLog"/> audit table.
/// </summary>
public sealed class LoggedPushTransport : IPushTransport
{
    private readonly IPushTransport _inner;
    private readonly INotificationLogRepository _logRepo;
    private readonly ILogger<LoggedPushTransport> _logger;

    public LoggedPushTransport(
        IPushTransport inner,
        INotificationLogRepository logRepo,
        ILogger<LoggedPushTransport> logger)
    {
        _inner = inner;
        _logRepo = logRepo;
        _logger = logger;
    }

    public async Task SendAsync(PushMessage message, CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;

        try
        {
            await _inner.SendAsync(message, cancellationToken);

            var logEntry = new NotificationLog
            {
                Id = Guid.NewGuid(),
                Channel = "Push",
                Provider = "Firebase",
                Recipient = message.FirebaseInstallationId,
                Status = NotificationStatus.Sent,
                OccurredAt = startedAt,
                CompletedAt = DateTime.UtcNow
                // EventId and RetryCount are not available at transport level;
                // they can be enriched later by the integration event handler if needed.
            };

            await _logRepo.AddAsync(logEntry, cancellationToken);
        }
        catch (Exception ex)
        {
            var logEntry = new NotificationLog
            {
                Id = Guid.NewGuid(),
                Channel = "Push",
                Provider = "Firebase",
                Recipient = message.FirebaseInstallationId,
                Status = NotificationStatus.Failed,
                FailureReason = ex.Message,
                OccurredAt = startedAt,
                CompletedAt = DateTime.UtcNow
            };

            await _logRepo.AddAsync(logEntry, cancellationToken);
            throw; // let the outbox / retry mechanism handle the failure
        }
    }
}