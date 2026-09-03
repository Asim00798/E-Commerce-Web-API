using E_Commerce.Infrastructure.Communication.Notifications.Contracts;
using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using E_Commerce.Infrastructure.Communication.Notifications.Messages;

namespace E_Commerce.Infrastructure.Communication.Notifications.Providers.Email.Transport;

/// <summary>
/// Decorates an <see cref="IEmailTransport"/> and records a <see cref="NotificationLog"/> entry after sending.
/// </summary>
public sealed class LoggedEmailTransport : IEmailTransport
{
    private readonly IEmailTransport _inner;
    private readonly INotificationLogRepository _logRepo;

    public LoggedEmailTransport(IEmailTransport inner, INotificationLogRepository logRepo)
    {
        _inner = inner;
        _logRepo = logRepo;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var log = new NotificationLog
        {
            Id = Guid.NewGuid(),
            Channel = "Email",
            Provider = "Smtp",
            Recipient = message.To,
            Status = NotificationStatus.Pending,
            OccurredAt = DateTime.UtcNow
        };

        try
        {
            await _inner.SendAsync(message, ct);
            log.Status = NotificationStatus.Sent;
            log.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            log.Status = NotificationStatus.Failed;
            log.FailureReason = ex.ToString();
            log.CompletedAt = DateTime.UtcNow;
            throw;
        }
        finally
        {
            // Fire-and-forget log (or you can save synchronously)
            await _logRepo.AddAsync(log, ct);
        }
    }
}
