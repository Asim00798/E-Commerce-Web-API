using E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEvents;
using E_Commerce.Application.BoundedContexts.Onboarding.Models;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Channels;

namespace E_Commerce.Application.BoundedContexts.Onboarding.IntegrationEventHandlers;

/// <summary>
/// Sends the registration verification SMS when a
/// <see cref="PhoneVerificationRequestedIntegrationEvent"/> is received.
/// The actual delivery is delegated to the SMS channel.
/// </summary>
public sealed class SendVerificationSmsHandler
    : IIntegrationEventHandler<PhoneVerificationRequestedIntegrationEvent>
{
    private readonly ISmsChannel _smsChannel;

    public SendVerificationSmsHandler(ISmsChannel smsChannel)
    {
        _smsChannel = smsChannel;
    }

    public async Task HandleAsync(
        PhoneVerificationRequestedIntegrationEvent evt,
        CancellationToken ct)
    {
        var model = new RegistrationVerificationSms
        {
            PhoneNumber = evt.PhoneNumber,
            VerificationCode = evt.Code
        };

        var request = new NotificationRequest<RegistrationVerificationSms>
        {
            UserId = evt.RegistrationId,
            Model = model
        };

        await _smsChannel.SendAsync(request, ct);
    }
}