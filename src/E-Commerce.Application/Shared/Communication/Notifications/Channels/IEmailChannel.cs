using E_Commerce.Application.Shared.Communication.Notifications.Models;

namespace E_Commerce.Application.Shared.Communication.Notifications.Abstractions;

/// <summary>
/// Sends an email notification.
/// Implementations load user preferences, compose the message, and deliver it
/// through an email transport.
/// </summary>
public interface IEmailChannel
{
    /// <summary>
    /// Sends a typed email notification to the user specified in <paramref name="request"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The notification model type (e.g., <see cref="OrderConfirmationEmail"/>,
    /// <see cref="PasswordResetEmail"/>).
    /// </typeparam>
    /// <param name="request">The request containing the user ID and the model.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendAsync<T>(NotificationRequest<T> request, CancellationToken ct = default)
        where T : INotificationModel;
}