using E_Commerce.Application.Modules.Identity.Credentials.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Channels;
using E_Commerce.Application.Shared.Communication.Notifications.Models;

namespace E_Commerce.Application.Modules.Identity.Credentials.IntegrationEventHandlers;

/// <summary>
/// Handles the password reset requested event by sending an email via the email channel.
/// </summary>
public sealed class SendPasswordResetEmailHandler
    : IIntegrationEventHandler<PasswordResetRequestedIntegrationEvent>
{
    private readonly IEmailChannel _emailChannel;

    public SendPasswordResetEmailHandler(IEmailChannel emailChannel)
        => _emailChannel = emailChannel;

    public async Task HandleAsync(PasswordResetRequestedIntegrationEvent evt, CancellationToken ct)
    {
        var model = new PasswordResetEmail
        {
            RecipientEmail = evt.Email,
            ResetToken = evt.ResetToken
        };

        await _emailChannel.SendAsync(new NotificationRequest<PasswordResetEmail>
        {
            UserId = evt.UserId,
            Model = model
        }, ct);
    }
}