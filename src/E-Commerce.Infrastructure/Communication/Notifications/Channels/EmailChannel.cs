using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Channels;
using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Infrastructure.Communication.Notifications.Contracts;
using E_Commerce.Infrastructure.Communication.Notifications.External.Email.Composers;
using E_Commerce.Infrastructure.Communication.Notifications.External.Email.Transport;

namespace E_Commerce.Infrastructure.Communication.Notifications.Channels;

/// <summary>
/// Handles email notification delivery by loading user preferences,
/// composing the email from a template, and sending it through the configured
/// transport.
/// </summary>
/// <remarks>
/// Implements <see cref="IEmailChannel"/> so that integration event handlers
/// depend on a single, channel‑specific abstraction. Preference checks are
/// performed internally – if email is disabled, the call is a no‑op.
/// </remarks>
public sealed class EmailChannel : IEmailChannel
{
    private readonly INotificationPreferencesRepository _preferencesRepo;
    private readonly EmailComposer _composer;
    private readonly IEmailTransport _transport;

    /// <summary>
    /// Initialises the email channel with all required dependencies.
    /// </summary>
    public EmailChannel(
        INotificationPreferencesRepository preferencesRepo,
        EmailComposer composer,
        IEmailTransport transport)
    {
        _preferencesRepo = preferencesRepo;
        _composer = composer;
        _transport = transport;
    }

    /// <inheritdoc />
    public async Task SendAsync<T>(NotificationRequest<T> request, CancellationToken ct = default)
        where T : IEmailNotificationModel
    {
        // 1. Respect user preferences
        var preferences = await _preferencesRepo.GetByUserIdAsync(request.UserId, ct);
        if (preferences?.AllowEmail is false)
            return;   // email disabled – nothing to do

        // 2. Compose the email message from template + model
        var emailMessage = await _composer.ComposeAsync(request.Model, ct);

        // 3. Send the email
        await _transport.SendAsync(emailMessage, ct);
    }
}