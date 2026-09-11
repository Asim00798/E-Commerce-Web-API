using Microsoft.AspNetCore.Builder;
using Serilog;

namespace E_Commerce.Infrastructure.Observability.Logging;

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
            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }
}