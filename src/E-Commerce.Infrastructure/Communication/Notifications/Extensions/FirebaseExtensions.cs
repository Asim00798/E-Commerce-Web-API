using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using E_Commerce.Infrastructure.Communication.Notifications.Options;

namespace E_Commerce.Infrastructure.Communication.Notifications.Extensions;

public static class FirebaseExtensions
{
    public static IServiceCollection AddFirebaseMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var pushOptions = configuration
            .GetSection("Push")
            .Get<PushOptions>();

        var serviceProvider = services.BuildServiceProvider();

        var logger = serviceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("FirebaseMessaging");

        FirebaseApp app;

        if (string.IsNullOrWhiteSpace(pushOptions?.CredentialFilePath))
        {
            logger.LogWarning(
                "Firebase credential path is missing. " +
                "Using Application Default Credentials.");

            app = FirebaseApp.Create();
        }
        else
        {
            logger.LogInformation(
                "Initializing Firebase using service account credential file: {CredentialPath}",
                pushOptions.CredentialFilePath);

            var credential = CredentialFactory
                .FromFile<ServiceAccountCredential>(
                    pushOptions.CredentialFilePath)
                .ToGoogleCredential();

            app = FirebaseApp.Create(new AppOptions
            {
                Credential = credential
            });

            logger.LogInformation(
                "Firebase initialized successfully using service account credentials.");
        }

        services.AddSingleton(
            FirebaseMessaging.GetMessaging(app));

        logger.LogInformation(
            "Firebase Messaging service registered successfully.");

        return services;
    }
}