using E_Commerce.Application.Shared.Communication.Notifications.Models;
using E_Commerce.Application.Shared.Communication.Notifications.Persistence;
using E_Commerce.Application.Shared.Models;
using MediatR;

namespace E_Commerce.Application.Modules.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, PagedList<UserNotificationDto>>
{
    private readonly IUserNotificationRepository _repository;

    public GetNotificationsQueryHandler(IUserNotificationRepository repository)
        => _repository = repository;

    public async Task<PagedList<UserNotificationDto>> Handle(
        GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;
        var items = await _repository.GetByUserIdAsync(request.UserId, skip, request.PageSize, cancellationToken);
        var totalCount = await _repository.GetTotalCountAsync(request.UserId, cancellationToken);

        return new PagedList<UserNotificationDto>(
            items, totalCount, request.Page, request.PageSize);
    }
}