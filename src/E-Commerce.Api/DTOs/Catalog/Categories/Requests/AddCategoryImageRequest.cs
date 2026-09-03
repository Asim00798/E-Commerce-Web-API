using Microsoft.AspNetCore.Http;

namespace E_Commerce.Api.DTOs.Catalog.Categories.Requests;

public sealed class AddCategoryImageRequest
{
    public IFormFile Image { get; set; } = null!;
}
