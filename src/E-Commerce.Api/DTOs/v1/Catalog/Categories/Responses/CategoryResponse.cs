namespace E_Commerce.Api.DTOs.v1.Catalog.Categories.Responses;

public sealed class CategoryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid? ParentCategoryId { get; init; }
    public IReadOnlyList<Guid> ImageFileIds { get; init; } = Array.Empty<Guid>();
}
