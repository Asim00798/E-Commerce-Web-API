using E_Commerce.Application.Shared.Communication.Notifications.Persistence;
using MediatR;

namespace E_Commerce.Application.Modules.Notifications.Commands.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandHandler
    : IRequestHandler<MarkNotificationAsReadCommand>
{
    private readonly IUserNotificationRepository _repository;

    public MarkNotificationAsReadCommandHandler(IUserNotificationRepository repository)
        => _repository = repository;

    public async Task Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        await _repository.MarkAsReadAsync(request.NotificationId, cancellationToken);
    }
}