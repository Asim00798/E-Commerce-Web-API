using Microsoft.AspNetCore.SignalR;

namespace E_Commerce.Api.Hubs;

/// <summary>
/// Minimal SignalR hub used for real‑time user notifications.
/// No business logic – only serves as the endpoint for client connections.
/// </summary>
public class NotificationHub : Hub
{}
