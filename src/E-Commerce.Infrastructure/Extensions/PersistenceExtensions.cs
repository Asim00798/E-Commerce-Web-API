using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Infrastructure.Persistence.Extensions;

/// <summary>
/// Registers the write-side EF Core DbContext.
///
/// The migration assembly is explicitly set to the Infrastructure assembly,
/// so migrations are discovered from <c>E-Commerce.Infrastructure/Persistence/Migrations/</c>
/// regardless of which project is the startup project.
/// </summary>
public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        return services;
    }
}