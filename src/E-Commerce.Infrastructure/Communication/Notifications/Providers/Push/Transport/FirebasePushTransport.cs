using FirebaseAdmin.Messaging;
using E_Commerce.Infrastructure.Communication.Notifications.Contracts;
using E_Commerce.Infrastructure.Communication.Notifications.Messages;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Infrastructure.Communication.Notifications.Providers.Push.Transport;

public sealed class FirebasePushTransport : IPushTransport
{
    private readonly FirebaseMessaging _messaging;
    private readonly IPushDeviceRepository _deviceRepo;
    private readonly ILogger<FirebasePushTransport> _logger;

    public FirebasePushTransport(
        FirebaseMessaging messaging,
        IPushDeviceRepository deviceRepo,
        ILogger<FirebasePushTransport> logger)
    {
        _messaging = messaging;
        _deviceRepo = deviceRepo;
        _logger = logger;
    }

    public async Task SendAsync(PushMessage message, CancellationToken cancellationToken = default)
    {
        var firebaseMessage = new Message
        {
            Fid = message.FirebaseInstallationId,
            Notification = new Notification
            {
                Title = message.Title,
                Body = message.Body
            }
        };

        try
        {
            await _messaging.SendAsync(firebaseMessage, cancellationToken);
        }
        catch (FirebaseMessagingException ex)
            when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                  ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
        {
            _logger.LogWarning("Deactivating invalid FID {Fid}: {Error}",
                message.FirebaseInstallationId, ex.MessagingErrorCode);
            await _deviceRepo.DeactivateByFidAsync(message.FirebaseInstallationId, CancellationToken.None);
        }
        // Other exceptions propagate → Outbox retries will handle them.
    }
}