using E_Commerce.Infrastructure.Communication.Notifications.Messages;

namespace E_Commerce.Infrastructure.Communication.Notifications.External.Push.Transport;

public interface IPushTransport
{
    Task SendAsync(PushMessage message, CancellationToken cancellationToken = default);
}