using E_Commerce.ReadModel.Features.CustomerProfile.Queries;
using E_Commerce.ReadModel.Features.EmployeeProfile.Queries;
using E_Commerce.ReadModel.Infrastructure.Connections;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.ReadModel.Infrastructure.DependencyInjection;

public static class ReadModelServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ReadModel services.
    /// Does not execute database migrations.
    /// </summary>
    public static IServiceCollection AddReadModel(
        this IServiceCollection services)
    {
        services.AddSingleton<
            IReadDbConnectionFactory,
            SqlReadDbConnectionFactory>();

        services.AddScoped<
            ICustomerProfileQueryService,
            CustomerProfileQueryService>();

        services.AddScoped<
            IEmployeeProfileQueryService,
            EmployeeProfileQueryService>();

        return services;
    }
}