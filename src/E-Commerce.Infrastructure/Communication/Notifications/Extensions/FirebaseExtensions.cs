using E_Commerce.Infrastructure.Communication.Notifications.Options;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Infrastructure.Communication.Notifications.Extensions;

public static class FirebaseExtensions
{
    public static IServiceCollection AddFirebaseMessaging(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddSingleton<FirebaseMessaging>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>()
                .CreateLogger("FirebaseMessaging");

            var pushOptions = configuration
                .GetSection(PushOptions.SectionName)
                .Get<PushOptions>();

            FirebaseApp app;

            if (string.IsNullOrWhiteSpace(pushOptions?.CredentialFilePath))
            {
                logger.LogWarning(
                    "Firebase credential path is missing. Using Application Default Credentials.");
                app = FirebaseApp.Create();
            }
            else
            {
                logger.LogInformation(
                    "Initializing Firebase using service account credential file: {CredentialPath}",
                    pushOptions.CredentialFilePath);

                var credential = CredentialFactory
                    .FromFile<ServiceAccountCredential>(pushOptions.CredentialFilePath)
                    .ToGoogleCredential();

                app = FirebaseApp.Create(new AppOptions { Credential = credential });
            }

            return FirebaseMessaging.GetMessaging(app);
        });

        return services;
    }
}