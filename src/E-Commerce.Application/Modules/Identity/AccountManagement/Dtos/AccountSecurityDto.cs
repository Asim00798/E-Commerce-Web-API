using E_Commerce.Application.Shared.Security.Identity;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Dtos;

/// <summary>
/// Safe security state of an account. Does not expose secrets.
/// </summary>
public sealed class AccountSecurityDto
{
    public bool EmailConfirmed { get; init; }
    public bool PhoneNumberConfirmed { get; init; }
    public bool TwoFactorEnabled { get; init; }
    public bool IsLockedOut { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
    public bool HasPassword { get; init; }
    public IReadOnlyList<string> ExternalLoginProviders { get; init; } = Array.Empty<string>();
    public AccountStatus AccountStatus { get; init; }
    public DateTime? PasswordLastChangedAtUtc { get; init; }
}