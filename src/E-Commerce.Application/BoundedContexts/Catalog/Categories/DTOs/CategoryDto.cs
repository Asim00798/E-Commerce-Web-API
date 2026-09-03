namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;

public sealed class CategoryDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public Guid? ParentCategoryId { get; init; }

    public IReadOnlyList<Guid> ImageFileIds { get; init; } = Array.Empty<Guid>();
}