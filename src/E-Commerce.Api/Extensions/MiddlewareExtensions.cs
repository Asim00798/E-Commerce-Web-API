using E_Commerce.Api.Middleware;
using Serilog;

namespace E_Commerce.Api.Extensions;

public static class MiddlewareExtensions
{
    public static void UseGlobalExceptionMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
    }

    public static void UseCorrelationIdMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
