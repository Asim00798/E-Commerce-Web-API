using Asp.Versioning;
using E_Commerce.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Api.Extensions;

public static class VersioningExtensions
{
    public static IServiceCollection AddApiVersioningConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var versioningOptions = configuration
            .GetSection(VersioningOptions.SectionName)
            .Get<VersioningOptions>()
            ?? new VersioningOptions();

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(
                    versioningOptions.DefaultMajorVersion,
                    versioningOptions.DefaultMinorVersion);

                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }
}