using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Channels;
using E_Commerce.Infrastructure.Communication.Notifications.Contracts;
using E_Commerce.Infrastructure.Communication.Notifications.Providers.Sms.Composers;
using E_Commerce.Infrastructure.Communication.Notifications.Providers.Sms.Transport;

namespace E_Commerce.Infrastructure.Communication.Notifications.Channels;

public sealed class SmsChannel : ISmsChannel
{
    private readonly SmsComposer _composer;
    private readonly ISmsTransport _transport;
    private readonly INotificationPreferencesRepository _preferencesRepo;

    public SmsChannel(
        SmsComposer composer,
        ISmsTransport transport,
        INotificationPreferencesRepository preferencesRepo)
    {
        _composer = composer;
        _transport = transport;
        _preferencesRepo = preferencesRepo;
    }

    public async Task SendAsync<T>(NotificationRequest<T> request, CancellationToken ct = default)
        where T : ISmsNotificationModel
    {
        var preferences = await _preferencesRepo.GetByUserIdAsync(request.UserId, ct);
        if (preferences?.AllowSms is false)
            return;   // SMS disabled – nothing to do

        var smsMessage = await _composer.ComposeAsync(request.Model, ct);
        await _transport.SendAsync(smsMessage, ct);
    }
}