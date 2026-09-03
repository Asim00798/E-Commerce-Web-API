using System.Security.Claims;
using E_Commerce.Application.Shared.Constants;

namespace E_Commerce.Api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Items[ContextKeys.CorrelationId] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        // Get user ID from claims
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [ContextKeys.CorrelationId] = correlationId,
            ["UserId"] = userId
        }))
        {
            await _next(context);
        }
    }
}
