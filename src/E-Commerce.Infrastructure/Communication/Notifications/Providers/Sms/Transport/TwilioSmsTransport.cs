using E_Commerce.Infrastructure.Communication.Notifications.Messages;
using E_Commerce.Infrastructure.Communication.Notifications.Options;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace E_Commerce.Infrastructure.Communication.Notifications.Providers.Sms.Transport;

public sealed class TwilioSmsTransport : ISmsTransport
{
    private readonly SmsOptions _options;
    private readonly ILogger<TwilioSmsTransport> _logger;

    public TwilioSmsTransport(IOptions<SmsOptions> options, ILogger<TwilioSmsTransport> logger)
    {
        _options = options.Value;
        _logger = logger;
        TwilioClient.Init(_options.AccountSid, _options.AuthToken);
    }

    public async Task SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending SMS to {Recipient} via Twilio", message.PhoneNumber);

        // Respect cancellation before the call (e.g., timeout via linked token).
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var response = await MessageResource.CreateAsync(
                to: new PhoneNumber(message.PhoneNumber),
                from: new PhoneNumber(_options.FromNumber),
                body: message.Text);

            _logger.LogInformation("SMS to {Recipient} sent successfully (SID: {Sid})",
                message.PhoneNumber, response.Sid);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("SMS send cancelled for {Recipient}", message.PhoneNumber);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Recipient}: {ErrorMessage}",
                message.PhoneNumber, ex.Message);
            throw;
        }
    }
}