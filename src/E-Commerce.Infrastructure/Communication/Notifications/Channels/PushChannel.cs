using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Models;
using E_Commerce.Infrastructure.Communication.Notifications.Contracts;
using E_Commerce.Infrastructure.Communication.Notifications.External.Push.Composers;
using E_Commerce.Infrastructure.Communication.Notifications.External.Push.Transport;
using E_Commerce.Infrastructure.Communication.Notifications.Messages;

namespace E_Commerce.Infrastructure.Communication.Notifications.Channels;

/// <summary>
/// Handles push notification delivery for a user by loading preferences,
/// composing the payload, and sending to all active devices.
/// </summary>
/// <remarks>
/// Implements <see cref="IPushChannel"/> so that integration event handlers
/// depend on a single, channel‑specific abstraction. Preference checks are
/// performed internally – if push is disabled, the call is a no‑op.
/// </remarks>
public sealed class PushChannel : IPushChannel
{
    private readonly INotificationPreferencesRepository _preferencesRepo;
    private readonly PushComposer _composer;
    private readonly IPushDeviceRepository _deviceRepo;
    private readonly IPushTransport _transport;

    /// <summary>
    /// Initialises the push channel with all required dependencies.
    /// </summary>
    public PushChannel(
        INotificationPreferencesRepository preferencesRepo,
        PushComposer composer,
        IPushDeviceRepository deviceRepo,
        IPushTransport transport)
    {
        _preferencesRepo = preferencesRepo;
        _composer = composer;
        _deviceRepo = deviceRepo;
        _transport = transport;
    }

    /// <inheritdoc />
    public async Task SendAsync(NotificationRequest<PushNotification> request, CancellationToken ct = default)
    {
        // 1. Respect user preferences
        var preferences = await _preferencesRepo.GetByUserIdAsync(request.UserId, ct);
        if (preferences?.AllowPush is false)
            return;   // push disabled – nothing to do

        var model = request.Model;

        // 2. Compose the push payload once (title + body)
        var composed = await _composer.ComposeAsync(model, ct);

        // 3. Retrieve all active devices for the user
        var devices = await _deviceRepo.GetActiveDevicesAsync(request.UserId, ct);

        // 4. Send to each device individually
        foreach (var device in devices)
        {
            var pushMessage = new PushMessage
            {
                FirebaseInstallationId = device.FirebaseInstallationId,
                Title = composed.Title,
                Body = composed.Body
            };

            await _transport.SendAsync(pushMessage, ct);
        }
    }
}