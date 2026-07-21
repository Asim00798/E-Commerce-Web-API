using E_Commerce.Infrastructure.Communication.Notifications.Entities;

namespace E_Commerce.Infrastructure.Communication.Notifications.Contracts;

/// <summary>
/// Persistence abstraction for notification audit logs.
/// </summary>
public interface INotificationLogRepository
{
    Task AddAsync(NotificationLog log, CancellationToken cancellationToken = default);
}