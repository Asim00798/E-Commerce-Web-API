using E_Commerce.Application.Shared.Observability.Tracing;
using OpenTelemetry.Trace;

namespace E_Commerce.Infrastructure.Observability.Tracing;

public static class TracingRegistrationExtensions
{
    /// <summary>
    /// Registers OpenTelemetry tracing, ASP.NET Core instrumentation,
    /// and the OTLP exporter. Also registers the trace context implementation.
    /// </summary>
    public static IServiceCollection AddApplicationTracing(this IServiceCollection services)
    {
        services.AddScoped<ITraceContext, OpenTelemetryTraceContext>();

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(ECommerceActivitySource.Instance.Name)
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter();
            });

        return services;
    }
}