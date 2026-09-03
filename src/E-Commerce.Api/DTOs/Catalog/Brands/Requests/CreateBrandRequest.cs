using Microsoft.AspNetCore.Http;

namespace E_Commerce.Api.DTOs.Catalog.Brands.Requests;

public sealed class CreateBrandRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IFormFile Logo { get; set; } = null!;
}
