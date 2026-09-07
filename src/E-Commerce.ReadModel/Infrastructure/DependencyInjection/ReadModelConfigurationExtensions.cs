using E_Commerce.ReadModel.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace E_Commerce.ReadModel.Infrastructure.DependencyInjection;

public static class ReadModelConfigurationExtensions
{
    /// <summary>
    /// Binds ReadModelOptions from the "ReadModel" configuration section
    /// and registers the custom validator.
    /// </summary>
    public static IServiceCollection AddReadModelConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<ReadModelOptions>()
            .Bind(configuration.GetSection("ReadModel"))
            .ValidateOnStart();

        services.AddSingleton<
            IValidateOptions<ReadModelOptions>,
            ReadModelOptionsValidator>();

        return services;
    }
}