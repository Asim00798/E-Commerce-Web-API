using E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEvents;
using E_Commerce.Application.BoundedContexts.Onboarding.Models;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Channels;

namespace E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEventHandlers;

/// <summary>
/// Sends the registration verification email when an
/// <see cref="EmailVerificationRequestedIntegrationEvent"/> is received.
/// The actual delivery is delegated to the email channel.
/// </summary>
public sealed class SendVerificationEmailHandler
    : IIntegrationEventHandler<EmailVerificationRequestedIntegrationEvent>
{
    private readonly IEmailChannel _emailChannel;

    public SendVerificationEmailHandler(IEmailChannel emailChannel)
    {
        _emailChannel = emailChannel;
    }

    public async Task HandleAsync(
        EmailVerificationRequestedIntegrationEvent evt,
        CancellationToken ct)
    {
        var model = new RegistrationVerificationEmail
        {
            RecipientEmail = evt.Email,
            VerificationCode = evt.Code
        };

        var request = new NotificationRequest<RegistrationVerificationEmail>
        {
            UserId = evt.RegistrationId,
            Model = model
        };

        await _emailChannel.SendAsync(request, ct);
    }
}