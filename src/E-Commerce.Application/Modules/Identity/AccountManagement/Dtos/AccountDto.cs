using E_Commerce.Application.Shared.Security.Identity;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Dtos;

/// <summary>
/// Safe, non‑sensitive account information returned to clients.
/// </summary>
public sealed class AccountDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public AccountStatus AccountStatus { get; init; }
    public bool EmailConfirmed { get; init; }
    public bool PhoneNumberConfirmed { get; init; }
    public bool TwoFactorEnabled { get; init; }
}