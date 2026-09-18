namespace E_Commerce.Application.Shared.Security.Authorization.Contracts;

/// <summary>
/// Declares a system role and the permissions it receives.
///
/// Implementing classes define one role. The role's <see cref="Name"/> must be
/// a constant from <c>SystemRoles</c>. The set returned by
/// <see cref="GetPermissions"/> may include <see cref="AllPermissions"/> as a
/// wildcard, which expands to every permission currently declared in the
/// codebase on each seed run.
///
/// Role classes are discovered via Scrutor. Adding a role is adding a class;
/// removing a role is removing a class.
/// </summary>
public interface IRole
{
    /// <summary>
    /// Wildcard: expands to every permission currently declared.
    /// </summary>
    const string AllPermissions = "*";

    /// <summary>
    /// The role's identifier. Must equal one of the constants declared in
    /// <see cref="Roles.SystemRoles"/>.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Permission names the role receives. Concrete names or the wildcard.
    /// </summary>
    IEnumerable<string> GetPermissions();
}