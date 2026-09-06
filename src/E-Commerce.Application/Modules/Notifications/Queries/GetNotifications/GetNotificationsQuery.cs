using E_Commerce.Application.Shared.Communication.Notifications.Models;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Notifications.Queries.GetNotifications;

/// <summary>
/// Returns a paged list of in‑app notifications for a user.
/// </summary>
public record GetNotificationsQuery(Guid UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedList<UserNotificationDto>>;