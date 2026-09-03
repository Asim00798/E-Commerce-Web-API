using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Infrastructure.Files.Configuration;
using E_Commerce.Infrastructure.Files.Services;
using E_Commerce.Infrastructure.Files.Storage;
using E_Commerce.Infrastructure.Persistence.Modules.Files.Repositories;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Files.Extensions;

public static class FileStorageInfrastructureExtensions
{
    public static IServiceCollection AddFileStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(FileStorageOptions.SectionName)
            .Get<FileStorageOptions>() ?? new FileStorageOptions();

        services.AddOptions<FileStorageOptions>()
            .Bind(configuration.GetSection(FileStorageOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<FileStorageOptions>, FileStorageOptionsValidator>();

        switch (options.Provider?.ToLowerInvariant())
        {
            case "local":
                services.Configure<LocalFileStorageOptions>(
                    configuration.GetSection($"{FileStorageOptions.SectionName}:Local"));
                services.AddScoped<IFileStorageProvider, LocalFileStorageProvider>();
                break;

            default:
                throw new InvalidOperationException(
                    $"Unknown file storage provider '{options.Provider}'. Valid providers: 'Local'.");
        }

        services.AddScoped<IFileService, FileService>();
        services.AddScoped<StoredFileRepository>();
        services.AddScoped<FileStorageCleanupService>();

        services.AddScoped<IFileContentInspector, FileContentInspector>();

        return services;
    }
}