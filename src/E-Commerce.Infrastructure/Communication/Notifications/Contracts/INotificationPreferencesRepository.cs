using E_Commerce.Infrastructure.Communication.Notifications.Entities;

namespace E_Commerce.Infrastructure.Communication.Notifications.Contracts;

/// <summary>
/// Repository for loading user notification preferences.
/// </summary>
public interface INotificationPreferencesRepository
{
    Task<NotificationPreferences?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}