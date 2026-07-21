using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Models;
using Microsoft.AspNetCore.SignalR;

namespace E_Commerce.Infrastructure.Communication.Realtime.Publishers;

/// <summary>
/// Implements <see cref="IRealtimeEventPublisher"/> using SignalR.
/// Uses a generic <see cref="IHubContext{Hub}"/> to avoid a dependency
/// on the concrete <see cref="NotificationHub"/> defined in the API layer.
/// </summary>
internal sealed class SignalRRealtimeEventPublisher : IRealtimeEventPublisher
{
    private readonly IHubContext<Hub> _hubContext;

    public SignalRRealtimeEventPublisher(IHubContext<Hub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishAsync(RealTimeMessage message, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.User(message.UserId.ToString())
            .SendAsync(message.Method, message.Payload, cancellationToken);
    }
}