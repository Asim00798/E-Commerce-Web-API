using E_Commerce.ReadModel.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.ReadModel;

public static class DependencyInjection
{
    public static IServiceCollection AddReadModel(this IServiceCollection services, IConfiguration configuration)
    {
        return ServiceCollectionExtensions.AddReadModel(services, configuration);
    }
}
