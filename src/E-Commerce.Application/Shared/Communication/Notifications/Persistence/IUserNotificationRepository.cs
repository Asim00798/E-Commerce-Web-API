using E_Commerce.Application.Shared.Communication.Notifications.Models;

namespace E_Commerce.Application.Shared.Communication.Notifications.Persistence;

/// <summary>
/// Persistence abstraction for user‑facing notifications.
/// The Application layer only works with <see cref="UserNotificationDto"/>.
/// </summary>
public interface IUserNotificationRepository
{
    Task<List<UserNotificationDto>> GetByUserIdAsync(
        Guid userId, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserNotificationDto dto, CancellationToken cancellationToken = default);
    Task<UserNotificationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
}