using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Security.ApiKey;

/// <summary>
/// ASP.NET Core authentication handler for API key-based authentication.
/// Validates the <c>X-Api-Key</c> header against configured keys.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // TODO: Extract X-Api-Key header, validate, return AuthenticateResult
        throw new NotImplementedException();
    }
}
