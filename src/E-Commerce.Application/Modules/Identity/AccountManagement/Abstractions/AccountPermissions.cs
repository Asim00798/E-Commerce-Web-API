namespace E_Commerce.Application.Modules.Identity.AccountManagement.Abstractions;

/// <summary>
/// Permission names for account management operations.
/// </summary>
public static class AccountPermissions
{
    public const string Activate = "Accounts.Activate";
    public const string Deactivate = "Accounts.Deactivate";
    public const string Lock = "Accounts.Lock";
    public const string Unlock = "Accounts.Unlock";
    public const string Read = "Accounts.Read";
}