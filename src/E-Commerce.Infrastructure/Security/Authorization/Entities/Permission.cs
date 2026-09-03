namespace E_Commerce.Infrastructure.Security.Authorization.Entities;

/// <summary>
/// Represents a named permission that can be assigned to one or more roles.
/// </summary>
public sealed class Permission
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}