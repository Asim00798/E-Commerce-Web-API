using MediatR;

namespace E_Commerce.Application.Modules.Notifications.Commands.MarkNotificationAsRead;

/// <summary>
/// Marks a single in‑app notification as read.
/// </summary>
public record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest;