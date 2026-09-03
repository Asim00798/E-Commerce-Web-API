using Microsoft.AspNetCore.Identity;
using E_Commerce.Application.Shared.Security.Identity;

namespace E_Commerce.Infrastructure.Security.Identity.Entities;

/// <summary>
/// Application user entity. Extends ASP.NET Core Identity's <see cref="IdentityUser{Guid}"/>
/// with application‑specific account state and metadata.
/// </summary>
public class User : IdentityUser<Guid>
{
    /// <summary>
    /// UTC timestamp when the user was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp when the password was last changed.
    /// Used for security policies and audit.
    /// </summary>
    public DateTime? PasswordLastChangedAtUtc { get; set; }

    /// <summary>
    /// Application‑level account lifecycle state.
    /// Independent from ASP.NET Identity lockout.
    /// </summary>
    public AccountStatus AccountStatus { get; set; } = AccountStatus.Active;
}