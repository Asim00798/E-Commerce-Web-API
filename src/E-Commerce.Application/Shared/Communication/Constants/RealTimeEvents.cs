namespace E_Commerce.Application.Shared.Communication.Notifications.Constants;

/// <summary>
/// Transport‑neutral names of client‑side methods that the server can invoke.
/// These are part of the application’s public API contract with real‑time clients.
/// </summary>
public static class RealTimeEvents
{
    public const string NewNotification = "NewNotification";
}