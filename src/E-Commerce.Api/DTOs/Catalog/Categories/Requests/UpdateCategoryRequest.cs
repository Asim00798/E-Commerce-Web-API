namespace E_Commerce.Api.DTOs.Catalog.Categories.Requests;

public sealed class UpdateCategoryRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool ClearParent { get; set; }
}
