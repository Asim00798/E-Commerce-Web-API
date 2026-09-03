namespace E_Commerce.Application.Modules.Authentication.Constants;

/// <summary>
/// Shared constants used by the Authentication module.
/// </summary>
public static class AuthenticationConstants
{
    /// <summary>
    /// Provider name used for Google external authentication.
    /// </summary>
    public const string GoogleProvider = "Google";

    /// <summary>
    /// Key used to store the protected user ID in the OAuth state.
    /// </summary>
    public const string LinkUserIdKey = "LinkUserId";

    /// <summary>
    /// Authentication scheme name for the temporary external cookie
    /// used during Google authentication redirects.
    /// </summary>
    public const string ExternalCookieScheme = "ExternalAuthentication";
}