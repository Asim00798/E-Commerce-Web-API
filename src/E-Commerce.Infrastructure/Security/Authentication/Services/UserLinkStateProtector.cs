using E_Commerce.Application.Modules.Authentication.Abstractions;
using Microsoft.AspNetCore.DataProtection;

namespace E_Commerce.Infrastructure.Security.Authentication.Services;

/// <summary>
/// Implements <see cref="IUserLinkStateProtector"/> using ASP.NET Core Data Protection.
/// </summary>
internal sealed class UserLinkStateProtector : IUserLinkStateProtector
{
    private readonly IDataProtector _protector;

    public UserLinkStateProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("LinkGoogleUserId");
    }

    public string Protect(Guid userId)
        => _protector.Protect(userId.ToString());

    public bool TryUnprotect(string protectedValue, out Guid userId)
    {
        userId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(protectedValue))
            return false;

        try
        {
            var value = _protector.Unprotect(protectedValue);
            return Guid.TryParse(value, out userId);
        }
        catch
        {
            return false;
        }
    }
}