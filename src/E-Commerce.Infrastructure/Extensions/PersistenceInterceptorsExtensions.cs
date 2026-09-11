using E_Commerce.Infrastructure.Persistence.Interceptors;

namespace E_Commerce.Infrastructure.Extensions;

public static class PersistenceInterceptorsExtensions
{
    /// <summary>
    /// Registers EF Core interceptors used for cross-cutting persistence logic.
    ///
    /// Interceptors are scoped because they may depend on other scoped services,
    /// such as ICurrentUser.
    ///
    /// Registration order is intentional:
    /// Validation → Timestamp → Audit/SoftDelete → Logging.
    /// </summary>
    public static IServiceCollection AddPersistenceInterceptors(
        this IServiceCollection services)
    {
        // Entity validation
        services.AddScoped<ValidationInterceptor>();

        // CreatedAt / UpdatedAt handling
        services.AddScoped<TimestampInterceptor>();

        // Soft-delete and audit logging
        services.AddScoped<AuditAndSoftDeleteInterceptor>();

        // Persistence logging
        services.AddScoped<LoggingInterceptor>();

        return services;
    }
}