using E_Commerce.Application.Shared.Communication.Notifications.Services;
using E_Commerce.Infrastructure.Communication.Notifications.Channels;
using E_Commerce.Infrastructure.Communication.Notifications.Contracts;
using E_Commerce.Infrastructure.Communication.Notifications.External.Email.Composers;
using E_Commerce.Infrastructure.Communication.Notifications.External.Email.Transport;
using E_Commerce.Infrastructure.Communication.Notifications.External.Push.Composers;
using E_Commerce.Infrastructure.Communication.Notifications.External.Push.Transport;
using E_Commerce.Infrastructure.Communication.Notifications.External.Sms.Composers;
using E_Commerce.Infrastructure.Communication.Notifications.External.Sms.Transport;
using E_Commerce.Infrastructure.Communication.Notifications.Options;
using E_Commerce.Infrastructure.Communication.Notifications.Rendering;
using E_Commerce.Infrastructure.Communication.Notifications.Services;
using E_Commerce.Infrastructure.Persistence.Modules.Notifications.Repositories;

namespace E_Commerce.Infrastructure.Communication.Notifications.Extensions;

public static class NotificationsInfrastructureExtensions
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Options
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.Configure<SmsOptions>(configuration.GetSection("Sms"));
        services.Configure<PushOptions>(configuration.GetSection("Push"));

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

        // Transport audit logging (optional decorator)
        services.Decorate<IEmailTransport, LoggedEmailTransport>();
        services.Decorate<ISmsTransport, LoggedSmsTransport>();   
        services.Decorate<IPushTransport, LoggedPushTransport>();
        // Inside AddNotificationInfrastructure (or wherever you register notification services)
       
        services.AddScoped<IPushDeviceRepository, PushDeviceRepository>();

        // Ensure the transport also receives IPushDeviceRepository (already wired)
        services.AddScoped<IPushTransport, FirebasePushTransport>();
        // Push device repository
        services.AddScoped<IPushDeviceRepository, PushDeviceRepository>();

        // Push registration service (bridging module to infrastructure)
        services.AddScoped<IPushDeviceRegistrationService, PushDeviceRegistrationService>();
        return services;
    }
}