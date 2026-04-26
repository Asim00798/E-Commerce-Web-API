namespace E_Commerce.Api.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        logger.LogInformation("HTTP {Method} {Path} started", context.Request.Method, context.Request.Path);
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await next(context);
        stopwatch.Stop();

        logger.LogInformation("HTTP {Method} {Path} finished in {ElapsedMilliseconds}ms", 
            context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
    }
}
