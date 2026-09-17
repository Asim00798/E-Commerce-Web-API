using E_Commerce.Application.Shared.Constants;

namespace E_Commerce.Api.Middlewares;

/// <summary>
/// Ensures every request carries a correlation ID.
///
/// Behavior:
/// - If the incoming request has a non-empty <c>X-Correlation-ID</c> header,
///   that value is used.
/// - Otherwise, a new GUID is generated.
/// - The value is stored in <see cref="HttpContext.Items"/> for downstream access,
///   added to the response header, and pushed onto the logging scope so that all
///   log entries for the request carry the same identifier.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Items[ContextKeys.CorrelationId] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [ContextKeys.CorrelationId] = correlationId
        }))
        {
            await _next(context);
        }
    }

    /// <summary>
    /// Reads the incoming correlation ID header and validates it. If the header is
    /// missing, empty, whitespace-only, or contains invalid characters, a new GUID
    /// is generated instead.
    /// </summary>
    private static string ResolveCorrelationId(HttpContext context)
    {
        var incoming = context.Request.Headers[HeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(incoming))
        {
            return Guid.NewGuid().ToString();
        }

        return incoming;
    }
}