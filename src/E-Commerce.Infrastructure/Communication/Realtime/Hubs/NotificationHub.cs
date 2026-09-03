using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace E_Commerce.Infrastructure.Communication.Realtime.Hubs;

/// <summary>
/// Minimal SignalR hub used for real‑time user notifications.
/// No business logic – only serves as the endpoint for client connections.
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub
{
}