using E_Commerce.Infrastructure.Communication.Notifications.Messages;
using E_Commerce.Infrastructure.Communication.Notifications.Options;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace E_Commerce.Infrastructure.Communication.Notifications.External.Sms.Transport;

public sealed class TwilioSmsTransport : ISmsTransport
{
    private readonly SmsOptions _options;

    public TwilioSmsTransport(IOptions<SmsOptions> options)
    {
        _options = options.Value;
        TwilioClient.Init(_options.AccountSid, _options.AuthToken);
    }

    public async Task SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        // Cancel early if requested – the Twilio call itself does not accept a token.
        cancellationToken.ThrowIfCancellationRequested();

        await MessageResource.CreateAsync(
            to: new PhoneNumber(message.PhoneNumber),
            from: new PhoneNumber(_options.FromNumber),
            body: message.Text);
    }
}