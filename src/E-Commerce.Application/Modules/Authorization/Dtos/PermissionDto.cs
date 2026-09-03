namespace E_Commerce.Application.Modules.Authorization.Dtos;

/// <summary>
/// Data transfer object representing a permission.
/// </summary>
public sealed class PermissionDto
{
    /// <summary>
    /// Permission identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Permission name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional description of the permission.
    /// </summary>
    public string? Description { get; init; }
}