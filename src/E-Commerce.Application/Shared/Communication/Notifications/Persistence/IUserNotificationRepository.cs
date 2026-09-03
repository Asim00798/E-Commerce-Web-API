using E_Commerce.Application.Shared.Communication.Notifications.Models;

namespace E_Commerce.Application.Shared.Communication.Notifications.Persistence;

/// <summary>
/// Persistence abstraction for user‑facing notifications.
/// The Application layer depends only on this contract.
/// Transaction ownership remains with <see cref="UnitOfWork"/>.
/// </summary>
public interface IUserNotificationRepository
{
    /// <summary>
    /// Persists a new notification.
    /// </summary>
    /// <param name="notification">The notification data.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task AddAsync(
        UserNotificationDto notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single notification by its unique identifier, or <c>null</c>.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The matching notification, or <c>null</c> if not found.</returns>
    Task<UserNotificationDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of notifications for the specified user, ordered by
    /// most recent first.
    /// </summary>
    /// <param name="userId">The target user's ID.</param>
    /// <param name="skip">Number of items to skip (for pagination).</param>
    /// <param name="take">Maximum number of items to return.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<IReadOnlyList<UserNotificationDto>> GetByUserIdAsync(
        Guid userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total number of notifications for a given user.
    /// </summary>
    /// <param name="userId">The target user's ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<int> GetTotalCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the number of unread notifications for a given user.
    /// </summary>
    /// <param name="userId">The target user's ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the specified notification as read. Has no effect if the
    /// notification does not exist or is already read.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task MarkAsReadAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}