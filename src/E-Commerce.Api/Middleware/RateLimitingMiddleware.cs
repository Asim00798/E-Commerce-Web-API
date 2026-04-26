namespace E_Commerce.Api.Middleware;

public class RateLimitingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Simple placeholder for rate limiting logic
        // In production, use Microsoft.AspNetCore.RateLimiting
        await next(context);
    }
}
