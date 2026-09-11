using Microsoft.AspNetCore.Http;

namespace E_Commerce.Api.DTOs.v1.Catalog.Products.Requests;

public sealed class AddProductImageRequest
{
    public IFormFile Image { get; set; } = null!;
}
