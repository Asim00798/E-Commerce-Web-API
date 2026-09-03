namespace E_Commerce.Api.Extensions;

/// <summary>
/// Adds modern security headers to every HTTP response.
/// </summary>
public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            // Prevent MIME‑type sniffing
            headers["X-Content-Type-Options"] = "nosniff";

            // Prevent clickjacking
            headers["X-Frame-Options"] = "DENY";

            // Strict referrer policy
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Disable browser features not used by the application
            headers["Permissions-Policy"] =
                "camera=(), microphone=(), geolocation=()";

            // Content Security Policy – adjust to your front‑end needs
            headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data:; " +
                "font-src 'self'; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none';";

            await next();
        });

        return app;
    }
}