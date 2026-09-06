using E_Commerce.Application.Shared.Communication.Notifications.Persistence;
using MediatR;

namespace E_Commerce.Application.Modules.Notifications.Queries.GetUnreadNotificationCount;

public sealed class GetUnreadNotificationCountQueryHandler
    : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly IUserNotificationRepository _repository;

    public GetUnreadNotificationCountQueryHandler(IUserNotificationRepository repository)
        => _repository = repository;

    public async Task<int> Handle(
        GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetUnreadCountAsync(request.UserId, cancellationToken);
    }
}