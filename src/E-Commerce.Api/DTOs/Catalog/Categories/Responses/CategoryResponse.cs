namespace E_Commerce.Api.DTOs.Catalog.Categories.Responses;

public sealed class CategoryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public IReadOnlyList<Guid> ImageFileIds { get; set; } = Array.Empty<Guid>();
}
