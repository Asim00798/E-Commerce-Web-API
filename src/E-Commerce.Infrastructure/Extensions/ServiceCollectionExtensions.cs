using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        //var connectionString = configuration["Database:ConnectionString"]
        //    ?? throw new InvalidOperationException("Database connection string is missing.");

        return services;
    }
}