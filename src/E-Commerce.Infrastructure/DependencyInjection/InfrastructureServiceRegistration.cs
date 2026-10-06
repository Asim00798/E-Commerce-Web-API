using E_Commerce.Infrastructure.Caching.Extensions;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Extensions;
using E_Commerce.Infrastructure.Communication.Notifications.Extensions;
using E_Commerce.Infrastructure.Communication.PostCommit.Extensions;
using E_Commerce.Infrastructure.Communication.Realtime.Extensions;
using E_Commerce.Infrastructure.Execution.Extensions;
using E_Commerce.Infrastructure.Extensions;
using E_Commerce.Infrastructure.Files.Extensions;
using E_Commerce.Infrastructure.Payment.Extensions;
using E_Commerce.Infrastructure.Persistence.Extensions;
using E_Commerce.Infrastructure.Scheduling.Extensions;
using E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Extensions;
using E_Commerce.Infrastructure.Security.Extensions;
using E_Commerce.Infrastructure.Shipping.Extensions;
using E_Commerce.Infrastructure.Stock.Extensions;
using E_Commerce.Infrastructure.Time.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace E_Commerce.Infrastructure.DependencyInjection;

/// <summary>
/// Entry point for registering all Infrastructure services into the DI container.
/// </summary>
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly infrastructureAssembly)
    {
        // Persistence — DbContext must be registered before anything that consumes it
        services.AddPersistence(configuration);
        services.AddMigrationOptions(configuration);
        services.AddPersistenceInterceptors();

        // Repositories — Scrutor scan over the Infrastructure assembly
        services.AddRepositories(infrastructureAssembly);

        // Execution context — correlation ID and current-user resolution
        services.AddExecutionContext();

        // Post-commit callbacks — used by Notifications for real-time hints
        services.AddPostCommitProcessor();

        // Transactional Outbox — writer, dispatcher, serializer, dispatch service
        services.AddOutboxMessaging();

        // Caching — Redis cache-aside
        services.AddRedisCaching(configuration);

        // Security — Identity, JWT bearer, authorization policies,
        // password hasher, refresh-token hasher, verification codes,
        // Google external authentication scheme
        services.AddSecurityInfrastructure(configuration);

        // Data Seeding — startup reconciliation of permissions, roles,
        // role-permission mappings, and the seed administrator.
        // Registered as an IHostedService; runs during host start, after
        // EF migrations complete.
        services.AddDataSeeding(configuration);

        // Stock — Catalog stock service
        services.AddStockInfrastructure();

        // Ordering — pending order cleanup service
        services.AddOrderingInfrastructure();

        // Shipping — location service and shipping fee calculator
        services.AddShippingInfrastructure(configuration);

        // Time — clock service
        services.AddTimeInfrastructure();

        // Payment — Paymob adapter
        services.AddFinancePayment(configuration);

        // File Storage — local provider, cleanup, content inspection
        services.AddFileStorage(configuration);

        // Notifications — email, SMS, push channels + transports + composers
        services.AddNotificationInfrastructure(configuration);

        // Firebase — required by FirebasePushTransport
        services.AddFirebaseMessaging(configuration);

        // Real-time — SignalR hub, realtime publisher, JWT bearer query-string token extraction
        services.AddSignalRRealTimeInfrastructure();

        // Scheduling — Hangfire storage, dispatcher, recurring job bootstrap
        var hangfireConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is required for Hangfire.");

        services.AddSchedulingInfrastructure(hangfireConnectionString);

        return services;
    }
}