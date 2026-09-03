using E_Commerce.Infrastructure.Communication.Notifications.Contracts;
using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using E_Commerce.Infrastructure.Communication.Notifications.Messages;

namespace E_Commerce.Infrastructure.Communication.Notifications.Providers.Sms.Transport;

/// <summary>
/// Decorator around <see cref="ISmsTransport"/> that records every SMS attempt
/// in the <see cref="NotificationLog"/> audit table.
/// </summary>
public sealed class LoggedSmsTransport : ISmsTransport
{
    private readonly ISmsTransport _inner;
    private readonly INotificationLogRepository _logRepo;
    private readonly ILogger<LoggedSmsTransport> _logger;

    public LoggedSmsTransport(
        ISmsTransport inner,
        INotificationLogRepository logRepo,
        ILogger<LoggedSmsTransport> logger)
    {
        _inner = inner;
        _logRepo = logRepo;
        _logger = logger;
    }

    public async Task SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;

        try
        {
            await _inner.SendAsync(message, cancellationToken);

            var logEntry = new NotificationLog
            {
                Id = Guid.NewGuid(),
                Channel = "Sms",
                Provider = "Twilio",                 // adjust if you use another provider
                Recipient = message.PhoneNumber,    // the phone number in E.164 format
                Status = NotificationStatus.Sent,
                OccurredAt = startedAt,
                CompletedAt = DateTime.UtcNow
            };

            await _logRepo.AddAsync(logEntry, cancellationToken);
        }
        catch (Exception ex)
        {
            var logEntry = new NotificationLog
            {
                Id = Guid.NewGuid(),
                Channel = "Sms",
                Provider = "Twilio",
                Recipient = message.PhoneNumber,
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