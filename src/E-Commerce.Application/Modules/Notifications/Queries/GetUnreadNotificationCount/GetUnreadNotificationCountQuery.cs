using MediatR;

namespace E_Commerce.Application.Modules.Notifications.Queries.GetUnreadNotificationCount;

/// <summary>
/// Returns the number of unread notifications for a user.
/// </summary>
public record GetUnreadNotificationCountQuery(Guid UserId) : IRequest<int>;