using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Infrastructure.Communication.Notifications.Entities;
using E_Commerce.Infrastructure.Communication.Notifications.External.Sms.Composers;
using E_Commerce.Infrastructure.Communication.Notifications.External.Sms.Transport;

namespace E_Commerce.Infrastructure.Communication.Notifications.Channels;

public sealed class SmsChannel : ISmsChannel
{
    private readonly SmsComposer _composer;
    private readonly ISmsTransport _transport;

    public SmsChannel(SmsComposer composer, ISmsTransport transport)
    {
        _composer = composer;
        _transport = transport;
    }

    public async Task SendAsync<T>(T model, NotificationPreferences preferences, CancellationToken cancellationToken)
        where T : INotificationModel
    {
        if (!preferences.AllowSms) return;

        var smsMessage = await _composer.ComposeAsync(model, cancellationToken);
        await _transport.SendAsync(smsMessage, cancellationToken);
    }
}