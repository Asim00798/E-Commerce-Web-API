using E_Commerce.Infrastructure.Communication.Notifications.Messages;

namespace E_Commerce.Infrastructure.Communication.Notifications.Providers.Email.Transport;

public interface IEmailTransport
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}