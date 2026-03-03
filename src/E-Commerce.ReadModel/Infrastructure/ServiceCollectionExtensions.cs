using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace E_Commerce.ReadModel.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReadModel(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ReadDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IReadDbContext>(provider => provider.GetRequiredService<ReadDbContext>());
        services.AddScoped<IQueryBus, QueryBus>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}
