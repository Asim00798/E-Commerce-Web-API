using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Communication.Messaging.Outbox.Extensions;
using E_Commerce.Infrastructure.Identity.Services;
using E_Commerce.Infrastructure.Persistence.Common.Implementation;
using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Infrastructure.Persistence.Interceptors;
using E_Commerce.Infrastructure.Persistence.Modules.Catalog.Repositories;

namespace E_Commerce.Infrastructure.Extensions;

//// <summary>
/// Registers all Infrastructure‑layer services into the DI container.
/// This includes the EF Core <see cref="AppDbContext"/> (with cross‑cutting interceptors),
/// generic and domain‑specific repositories, the transactional <see cref="IUnitOfWork"/>,
/// the Outbox messaging subsystem, and the current‑user provider.
/// </summary>
/// <param name="services">The service collection to extend.</param>
/// <param name="configuration">Application configuration; must contain the database connection string.</param>
/// <returns>The same service collection for chaining.</returns>
/// <exception cref="InvalidOperationException">
/// Thrown when the connection string is missing from configuration.
/// </exception>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        // -----------------------------------------------------------------
        // 1. Database connection string
        // -----------------------------------------------------------------
        var connectionString = configuration["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Database connection string is missing.");

        // -----------------------------------------------------------------
        // 2. EF Core interceptors (cross‑cutting persistence logic)
        // Registered as scoped so they can consume other scoped services
        // (e.g., ICurrentUser inside AuditAndSoftDeleteInterceptor).
        // Execution order is important and is enforced when they are added
        // to the options pipeline below.
        // -----------------------------------------------------------------
        services.AddScoped<ValidationInterceptor>();          // entity validation
        services.AddScoped<TimestampInterceptor>();           // CreatedAt / UpdatedAt
        services.AddScoped<AuditAndSoftDeleteInterceptor>(); // soft‑delete + audit

        // -----------------------------------------------------------------
        // 3. DbContext
        // A single AppDbContext for all write‑side bounded contexts.
        // Interceptors are attached in the desired order:
        //   1. Validation (fail fast before any other work)
        //   2. Timestamps (accurate CreatedAt / UpdatedAt for audit)
        //   3. Audit + Soft‑Delete (captures final state)
        //   4. Logging (last, to capture the final state after all changes)
        // The migration history table is explicitly placed in the 'dbo' schema.
        // -----------------------------------------------------------------
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo"));
            options.AddInterceptors(
                sp.GetRequiredService<ValidationInterceptor>(),//1
                sp.GetRequiredService<TimestampInterceptor>(), //2
                sp.GetRequiredService<AuditAndSoftDeleteInterceptor>(), //3
                sp.GetRequiredService<LoggingInterceptor>() //4
            );
        });

        // -----------------------------------------------------------------
        // 4. Repositories
        // Generic repository for simple CRUD needs, and concrete domain
        // repositories that inherit from it while adding specialised queries.
        // All are registered against their abstractions to keep the
        // Application layer decoupled from Infrastructure.
        // -----------------------------------------------------------------
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));      // generic
        services.AddScoped<IProductRepository, ProductRepository>();          // catalog
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        // Additional bounded‑context repositories go here...

        // -----------------------------------------------------------------
        // 5. Unit of Work
        // Coordinates transaction boundaries and domain‑event dispatching
        // within a single atomic scope. It depends on IDomainEventDispatcher,
        // which is registered as part of the Outbox subsystem.
        // -----------------------------------------------------------------
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // -----------------------------------------------------------------
        // 6. Outbox & Messaging
        // Registers:
        //   - IDomainEventDispatcher  &  IIntegrationEventDispatcher
        //   - IOutboxMessageWriter
        //   - OutboxProcessor (BackgroundService)
        //   - Serialization, repository, and dispatch services
        // -----------------------------------------------------------------
        services.AddOutboxMessaging();

        // -----------------------------------------------------------------
        // 7. Identity & current user
        // Provides the current user context for audit and authorization
        // without coupling the Application layer to ASP.NET Core.
        // -----------------------------------------------------------------
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
   
    