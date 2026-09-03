namespace E_Commerce.Application.Modules.Authorization.Dtos;

/// <summary>
/// Data transfer object representing a role.
/// </summary>
public sealed class RoleDto
{
    /// <summary>
    /// Role identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Role name.
    /// </summary>
    public string Name { get; init; } = string.Empty;
}