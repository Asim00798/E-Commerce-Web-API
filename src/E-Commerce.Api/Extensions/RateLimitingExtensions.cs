using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace E_Commerce.Api.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddProductionRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("ip-fixed-window", CreateIpPolicy);
            options.AddPolicy("user-sliding-window", CreateUserPolicy);

            options.GlobalLimiter = CreateGlobalLimiter();

            options.OnRejected = HandleRejection;
        });

        return services;
    }

    public static IApplicationBuilder UseProductionRateLimiting(this IApplicationBuilder app)
    {
        return app.UseRateLimiter();
    }

    // =========================
    // Policies
    // =========================

    private static RateLimitPartition<string> CreateIpPolicy(HttpContext httpContext)
    {
        var ip = GetIp(httpContext);

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    }

    private static RateLimitPartition<string> CreateUserPolicy(HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: userId,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 50,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueLimit = 0
            });
    }

    private static PartitionedRateLimiter<HttpContext> CreateGlobalLimiter()
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(_ =>
        {
            return RateLimitPartition.GetConcurrencyLimiter(
                partitionKey: "global",
                factory: _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = 500,
                    QueueLimit = 50,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
        });
    }

    // =========================
    // Rejection Handler
    // =========================

    private static async ValueTask HandleRejection(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var logger = GetLogger(context);

        var ip = GetIp(context.HttpContext);
        var userId = GetUserId(context.HttpContext);
        var path = context.HttpContext.Request.Path;

        logger.LogWarning(
            "Rate limit triggered | IP: {IP} | User: {User} | Path: {Path}",
            ip, userId, path);

        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsync(
            """{"error":"Too many requests","status":429}""",
            cancellationToken);
    }

    // =========================
    // Helpers
    // =========================

    private static string GetIp(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static string GetUserId(HttpContext context)
        => context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

    private static ILogger GetLogger(OnRejectedContext context)
        => context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiting");
}