using Microsoft.AspNetCore.Http;

namespace E_Commerce.Api.DTOs.v1.Catalog.Categories.Requests;

public sealed class AddCategoryImageRequest
{
    public IFormFile Image { get; set; } = null!;
}
