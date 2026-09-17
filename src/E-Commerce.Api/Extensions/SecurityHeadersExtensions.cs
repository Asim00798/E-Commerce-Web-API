namespace E_Commerce.Api.Extensions;

/// <summary>
/// Registers middleware that adds security headers to every HTTP response.
///
/// Headers are applied unconditionally to all responses, including error responses,
/// because they are hardening measures that should always be present.
///
/// The Content-Security-Policy header is skipped for the Swagger UI paths because
/// Swashbuckle's default index.html contains an inline &lt;script&gt; block that
/// initializes the UI. With <c>script-src 'self'</c> and no <c>'unsafe-inline'</c>,
/// browsers block that block, and the page renders blank. Since Swagger is only
/// enabled in Development, skipping CSP for its paths does not affect production.
/// </summary>
public static class SecurityHeadersExtensions
{
    /// <summary>
    /// The Content-Security-Policy applied to every response except Swagger UI.
    /// </summary>
    /// <remarks>
    /// Directives:
    /// <list type="bullet">
    ///   <item><c>default-src 'self'</c> — fallback for any directive not listed below.</item>
    ///   <item><c>script-src 'self'</c> — scripts only from same origin. Blocks inline
    ///     scripts and <c>eval</c>.</item>
    ///   <item><c>style-src 'self' 'unsafe-inline'</c> — styles from same origin, plus
    ///     inline styles. <c>'unsafe-inline'</c> permits <c>&lt;style&gt;</c> blocks and
    ///     <c>style=""</c> attributes, which are commonly emitted by templating engines
    ///     and dynamically-generated content.</item>
    ///   <item><c>img-src 'self' data:</c> — images from same origin plus data URIs.
    ///     Data URIs are commonly used for icons and small inline assets.</item>
    ///   <item><c>font-src 'self'</c> — fonts only from same origin.</item>
    ///   <item><c>connect-src 'self'</c> — restricts XHR, fetch, WebSocket, and EventSource
    ///     targets to same origin. Enforced by the browser for documents that load
    ///     resources; not enforced for JSON responses consumed programmatically.</item>
    ///   <item><c>frame-ancestors 'none'</c> — modern replacement for X-Frame-Options.
    ///     Redundant with <c>X-Frame-Options: DENY</c>, but harmless and more explicit.</item>
    /// </list>
    /// </remarks>
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none';";

    /// <summary>
    /// Adds the security headers middleware to the request pipeline.
    /// Should be registered early in the pipeline so headers appear on error responses too.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            // Prevents browsers from MIME-sniffing a response away from the declared
            // content-type. Without this, a browser might interpret a text/plain response
            // as HTML or JavaScript.
            headers["X-Content-Type-Options"] = "nosniff";

            // Prevents the response from being embedded in a frame or iframe.
            // Defends against clickjacking.
            headers["X-Frame-Options"] = "DENY";

            // Controls how much referrer information is sent with cross-origin requests.
            // "strict-origin-when-cross-origin" sends the full URL for same-origin requests,
            // and only the origin (no path or query) for cross-origin requests.
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Restricts access to browser features.
            // Empty parentheses () disable the feature entirely.
            // Only includes features the API does not use; extend if needed.
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            // CSP is skipped for Swagger UI because Swashbuckle's default HTML page
            // includes an inline initialization script that the strict script-src
            // directive would block. Swagger is Development-only, so this exception
            // does not affect the production security posture.
            var isSwaggerUi = context.Request.Path.StartsWithSegments("/swagger");
            if (!isSwaggerUi)
            {
                headers["Content-Security-Policy"] = ContentSecurityPolicy;
            }

            await next();
        });

        return app;
    }
}