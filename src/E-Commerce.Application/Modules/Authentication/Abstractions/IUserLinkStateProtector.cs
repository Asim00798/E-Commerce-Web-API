namespace E_Commerce.Application.Modules.Authentication.Abstractions;

/// <summary>
/// Protects and unprotects the user ID that is carried through the
/// external authentication linking flow.
/// </summary>
public interface IUserLinkStateProtector
{
    /// <summary>
    /// Protects a user ID so it can safely be placed in OAuth state.
    /// </summary>
    string Protect(Guid userId);

    /// <summary>
    /// Unprotects a protected user ID. Returns false if the value is invalid or tampered.
    /// </summary>
    bool TryUnprotect(string protectedValue, out Guid userId);
}