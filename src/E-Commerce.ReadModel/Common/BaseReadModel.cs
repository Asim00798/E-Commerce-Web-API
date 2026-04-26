namespace E_Commerce.ReadModel.Common;

/// <summary>
/// Base class shared by all read models, providing common audit fields.
/// </summary>
public abstract class BaseReadModel
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
