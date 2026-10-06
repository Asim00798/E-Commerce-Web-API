using E_Commerce.Application.Shared.Communication.Notifications.Channels;
using E_Commerce.Application.Shared.Communication.Notifications.Services;
using E_Commerce.Infrastructure.Communication.Notifications.Channels;
using E_Commerce.Infrastructure.Communication.Notifications.Options;
using E_Commerce.Infrastructure.Communication.Notifications.Providers.Email.Composers;
using E_Commerce.Infrastructure.Communication.Notifications.Providers.Email.Transport;
using E_Commerce.Infrastructure.Communication.Notifications.Providers.Push.Composers;
using E_Commerce.Infrastructure.Communication.Notifications.Providers.Push.Transport;
using E_Commerce.Infrastructure.Communication.Notifications.Providers.Sms.Composers;
using E_Commerce.Infrastructure.Communication.Notifications.Providers.Sms.Transport;
using E_Commerce.Infrastructure.Communication.Notifications.Rendering;
using E_Commerce.Infrastructure.Communication.Notifications.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Infrastructure.Communication.Notifications.Extensions;

public static class NotificationsInfrastructureExtensions
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options with inline validation
        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Host),
                "Email:Host is required.")
            .Validate(o => o.Port > 0 && o.Port <= 65535,
                "Email:Port must be between 1 and 65535.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.SenderEmail),
                "Email:SenderEmail is required.")
            .Validate(o => o.TimeoutSeconds >= 1,
                "Email:TimeoutSeconds must be at least 1.")
            .ValidateOnStart();

        services.AddOptions<SmsOptions>()
            .Bind(configuration.GetSection(SmsOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.AccountSid),
                "Sms:AccountSid is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.AuthToken),
                "Sms:AuthToken is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.FromNumber),
                "Sms:FromNumber is required.")
            .ValidateOnStart();

        services.AddOptions<PushOptions>()
            .Bind(configuration.GetSection(PushOptions.SectionName))
            .ValidateOnStart();

        // Notification channels
        services.AddScoped<IEmailChannel, EmailChannel>();
        services.AddScoped<ISmsChannel, SmsChannel>();
        services.AddScoped<IPushChannel, PushChannel>();

        // Transports
        services.AddScoped<IEmailTransport, SmtpEmailTransport>();
        services.AddScoped<ISmsTransport, TwilioSmsTransport>();
        services.AddScoped<IPushTransport, FirebasePushTransport>();

        // Composers
        services.AddScoped<EmailComposer>();
        services.AddScoped<SmsComposer>();
        services.AddScoped<PushComposer>();

        // Renderer (singleton to avoid re‑compilation)
        services.AddSingleton(sp =>
            new RazorTemplateRenderer(
                Path.Combine(AppContext.BaseDirectory, "Communication", "Notifications", "Templates")));

        // Transport audit logging
        services.Decorate<IEmailTransport, LoggedEmailTransport>();
        services.Decorate<ISmsTransport, LoggedSmsTransport>();
        services.Decorate<IPushTransport, LoggedPushTransport>();

        // Push registration service
        services.AddScoped<IPushDeviceRegistrationService, PushDeviceRegistrationService>();

        // NOTE: Repositories (IPushDeviceRepository, INotificationLogRepository,
        // INotificationPreferencesRepository) are auto-discovered by
        // RepositoryRegistrationExtensions. No explicit registration needed.

        // NOTE: FirebaseApp / FirebaseMessaging singleton is registered by
        // AddFirebaseMessaging(configuration). The host composition root
        // must call both AddNotificationInfrastructure and AddFirebaseMessaging.

        return services;
    }
}