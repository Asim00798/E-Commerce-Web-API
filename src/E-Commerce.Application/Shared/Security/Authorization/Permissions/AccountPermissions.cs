namespace E_Commerce.Application.Shared.Security.Authorization.Permissions;

/// <summary>
/// Permission constants for account management operations.
/// These identifiers must exist in the <c>security.Permissions</c> table.
/// </summary>
public static class AccountPermissions
{
    public const string Activate = "Accounts.Activate";
    public const string Deactivate = "Accounts.Deactivate";
    public const string Lock = "Accounts.Lock";
    public const string Unlock = "Accounts.Unlock";
    public const string Read = "Accounts.Read";
}