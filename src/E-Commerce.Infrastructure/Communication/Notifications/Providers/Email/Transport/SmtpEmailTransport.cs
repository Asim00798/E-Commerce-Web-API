using E_Commerce.Infrastructure.Communication.Notifications.Messages;
using E_Commerce.Infrastructure.Communication.Notifications.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace E_Commerce.Infrastructure.Communication.Notifications.Providers.Email.Transport;

/// <summary>
/// SMTP implementation of <see cref="IEmailTransport"/> using MailKit.
/// Logs every attempt, enforces a timeout, distinguishes timeout from
/// caller cancellation, and guarantees a clean disconnect.
/// </summary>
public sealed class SmtpEmailTransport : IEmailTransport
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailTransport> _logger;

    public SmtpEmailTransport(
        IOptions<EmailOptions> options,
        ILogger<SmtpEmailTransport> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        LogSendStart(message);

        // Keep timeoutCts as a local variable so the catch filter can examine it.
        using var timeoutCts = new CancellationTokenSource(
            TimeSpan.FromSeconds(_options.TimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, timeoutCts.Token);
        var ct = linkedCts.Token;

        using var client = new SmtpClient();

        try
        {
            await ConnectAndAuthenticateAsync(client, ct);
            var mimeMessage = BuildMimeMessage(message);
            await SendMimeMessageAsync(client, mimeMessage, ct);

            LogSendSuccess(message);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested &&
                                                 !cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred – the caller did not request cancellation.
            LogTimeout(message);
            throw new TimeoutException(
                $"SMTP operation timed out after {_options.TimeoutSeconds} seconds.");
        }
        catch (OperationCanceledException)
        {
            // Caller requested cancellation (or shutdown).
            LogCallerCancellation(message);
            throw;
        }
        catch (Exception ex)
        {
            LogSendFailure(message, ex);
            throw;
        }
        finally
        {
            await DisconnectAsync(client);
        }
    }

    #region Helpers

    private void LogSendStart(EmailMessage message)
    {
        _logger.LogDebug(
            "Sending email to {Recipient} with subject '{Subject}' via {Host}:{Port}",
            message.To, message.Subject, _options.Host, _options.Port);
        _logger.LogInformation(
            "Sending email to {Recipient} via {Host}:{Port}",
            message.To, _options.Host, _options.Port);
    }

    private async Task ConnectAndAuthenticateAsync(SmtpClient client, CancellationToken ct)
    {
        await client.ConnectAsync(_options.Host, _options.Port, _options.SecureSocketOption, ct);

        if (!string.IsNullOrEmpty(_options.Username))
            await client.AuthenticateAsync(_options.Username, _options.Password, ct);
    }

    private static async Task SendMimeMessageAsync(SmtpClient client, MimeMessage mimeMessage, CancellationToken ct)
    {
        await client.SendAsync(mimeMessage, ct);
    }

    private void LogSendSuccess(EmailMessage message)
    {
        _logger.LogInformation("Email to {Recipient} sent successfully", message.To);
    }

    private void LogTimeout(EmailMessage message)
    {
        _logger.LogWarning(
            "Email to {Recipient} timed out after {TimeoutSeconds}s",
            message.To, _options.TimeoutSeconds);
    }

    private void LogCallerCancellation(EmailMessage message)
    {
        _logger.LogDebug("SMTP send cancelled for {Recipient}", message.To);
    }

    private void LogSendFailure(EmailMessage message, Exception ex)
    {
        _logger.LogError(ex,
            "Failed to send email to {Recipient}: {ErrorMessage}",
            message.To, ex.Message);
    }

    private static async Task DisconnectAsync(SmtpClient client)
    {
        if (client.IsConnected)
            await client.DisconnectAsync(true, CancellationToken.None);
    }

    private MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        mimeMessage.To.Add(new MailboxAddress(string.Empty, message.To));
        mimeMessage.Subject = message.Subject;
        mimeMessage.Body = new TextPart(message.IsHtml ? "html" : "plain")
        {
            Text = message.Body
        };
        return mimeMessage;
    }

    #endregion
}