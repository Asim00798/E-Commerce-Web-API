using E_Commerce.Application.Shared.Observability.Metrics;
using OpenTelemetry.Metrics;                                  // For WithMetrics, AddMeter, AddOtlpExporter

namespace E_Commerce.Infrastructure.Observability.Metrics;

public static class MetricsRegistrationExtensions
{
    /// <summary>
    /// Registers OpenTelemetry metrics, the application Meter, and typed metric classes.
    /// Configures ASP.NET Core instrumentation and OTLP exporter.
    /// </summary>
    public static IServiceCollection AddApplicationMetrics(this IServiceCollection services)
    {
        // Register the application Meter as singleton
        services.AddSingleton(ECommerceMeter.Instance);

        // Register typed metric classes
        services.AddSingleton<OrderMetrics>();

        // Register OpenTelemetry metrics pipeline
        services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddMeter(ECommerceMeter.Instance.Name)
                        .AddAspNetCoreInstrumentation()
                        .AddOtlpExporter();
                });

        return services;
    }
}