namespace E_Commerce.Application.Shared.Security.Identity;

/// <summary>
/// Application‑level account lifecycle state.
/// Independent from ASP.NET Core Identity lockout.
/// </summary>
public enum AccountStatus
{
    Active = 1,
    Deactivated = 2
}