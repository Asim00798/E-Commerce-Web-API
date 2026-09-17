using System.Threading.RateLimiting;
using E_Commerce.Api.Configuration;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Api.Extensions;

/// <summary>
/// Configures HTTP rate limiting for the API.
///
/// Rate limiting runs before authentication — this is intentional. HTTP-layer
/// rate limiting protects the authentication pipeline from abuse and partitions
/// by signals available pre-auth (IP, endpoint path). It cannot partition by
/// user identity, because authentication has not yet run.
///
/// Per-user quotas — if needed — belong at the business layer (inside handlers),
/// where <c>ICurrentUser</c> is populated. Do not attempt to enforce them here.
/// </summary>
public static class RateLimitingExtensions
{
    public static IServiceCollection AddProductionRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>()
            ?? new RateLimitingOptions();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            limiter.AddPolicy("ip-fixed-window", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetIp(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.Ip.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.Ip.WindowSeconds),
                        QueueLimit = options.Ip.QueueLimit,
                        AutoReplenishment = true
                    }));

            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                RateLimitPartition.GetConcurrencyLimiter(
                    partitionKey: "global",
                    factory: _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = options.Global.PermitLimit,
                        QueueLimit = options.Global.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            limiter.OnRejected = HandleRejection;
        });

        return services;
    }

    public static IApplicationBuilder UseProductionRateLimiting(this IApplicationBuilder app)
    {
        return app.UseRateLimiter();
    }

    private static async ValueTask HandleRejection(
        OnRejectedContext context,
        CancellationToken cancellationToken)
    {
        var http = context.HttpContext;
        var logger = http.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiting");

        logger.LogWarning(
            "Rate limit triggered | IP: {IP} | Path: {Path}",
            GetIp(http),
            http.Request.Path);

        http.Response.ContentType = "application/problem+json";
        http.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        await http.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc6585#section-4",
            title = "Too Many Requests",
            status = 429,
            detail = "Rate limit exceeded. Please try again later."
        }, cancellationToken);
    }

    private static string GetIp(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}