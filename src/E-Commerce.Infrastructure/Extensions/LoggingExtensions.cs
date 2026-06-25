using Microsoft.AspNetCore.Builder;
using Serilog;

namespace E_Commerce.Infrastructure.Extensions;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddInfrastructureLogging(this WebApplicationBuilder builder)
    {
        var appName = builder.Configuration["App:Name"] ?? "E-Commerce.Api";
        var environment = builder.Environment.EnvironmentName;
        var version = builder.Configuration["App:Version"] ?? "1.0.0";

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("AppName", appName)
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithProperty("Version", version)
            .WriteTo.Console()                                      // Console sink
            .WriteTo.File("logs/ecommerce-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024)
            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }
}