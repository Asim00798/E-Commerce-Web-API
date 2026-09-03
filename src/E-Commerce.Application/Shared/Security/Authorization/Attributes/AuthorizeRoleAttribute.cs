namespace E_Commerce.Application.Shared.Security.Authorization.Attributes;

/// <summary>
/// Specifies that a command requires the current user to be a member of the given role.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class AuthorizeRoleAttribute : Attribute
{
    /// <summary>
    /// The role name required to execute the command.
    /// </summary>
    public string Role { get; }

    public AuthorizeRoleAttribute(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be null or empty.", nameof(role));

        Role = role;
    }
}