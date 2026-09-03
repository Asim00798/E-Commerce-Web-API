using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Models;
using E_Commerce.Infrastructure.Communication.Realtime.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace E_Commerce.Infrastructure.Communication.Realtime.Publishers;

internal sealed class SignalRRealtimeEventPublisher : IRealtimeEventPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRRealtimeEventPublisher> _logger;

    public SignalRRealtimeEventPublisher(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRRealtimeEventPublisher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishAsync(RealTimeMessage message, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Publishing real‑time event. UserId={UserId}, Method={Method}",
            message.UserId, message.Method);

        try
        {
            await _hubContext.Clients.User(message.UserId.ToString())
                .SendAsync(message.Method, message.Payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish real‑time event. UserId={UserId}, Method={Method}",
                message.UserId, message.Method);
            throw;
        }
    }
}