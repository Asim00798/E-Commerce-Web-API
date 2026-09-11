using System.Security.Claims;
using System.Threading.RateLimiting;
using E_Commerce.Api.Configuration;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Api.Extensions;

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

            limiter.AddPolicy("user-sliding-window", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: GetUserId(context),
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = options.User.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.User.WindowSeconds),
                        SegmentsPerWindow = options.User.SegmentsPerWindow,
                        QueueLimit = options.User.QueueLimit
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
            "Rate limit triggered | IP: {IP} | User: {User} | Path: {Path}",
            GetIp(http),
            GetUserId(http),
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

    private static string GetUserId(HttpContext context)
        => context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
}