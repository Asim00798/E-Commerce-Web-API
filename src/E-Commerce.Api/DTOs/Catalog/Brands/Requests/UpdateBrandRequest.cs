using Microsoft.AspNetCore.Http;

namespace E_Commerce.Api.DTOs.Catalog.Brands.Requests;

public sealed class UpdateBrandRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public IFormFile? Logo { get; set; }
}
