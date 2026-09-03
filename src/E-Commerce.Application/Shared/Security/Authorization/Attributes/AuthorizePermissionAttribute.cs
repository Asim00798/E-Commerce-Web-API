namespace E_Commerce.Application.Shared.Security.Authorization.Attributes;

/// <summary>
/// Specifies that a command requires the current user to have the given permission.
/// Multiple attributes may be applied to require multiple permissions (AND semantics).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class AuthorizePermissionAttribute : Attribute
{
    /// <summary>
    /// The permission name required to execute the command.
    /// </summary>
    public string Permission { get; }

    public AuthorizePermissionAttribute(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException("Permission cannot be null or empty.", nameof(permission));

        Permission = permission;
    }
}