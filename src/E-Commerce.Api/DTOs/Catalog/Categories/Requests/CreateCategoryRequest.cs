namespace E_Commerce.Api.DTOs.Catalog.Categories.Requests;

public sealed class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
}
