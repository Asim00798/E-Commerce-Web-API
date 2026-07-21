using E_Commerce.Infrastructure.Communication.Notifications.Messages;

namespace E_Commerce.Infrastructure.Communication.Notifications.External.Sms.Transport;

public interface ISmsTransport
{
    Task SendAsync(SmsMessage message, CancellationToken cancellationToken = default);
}