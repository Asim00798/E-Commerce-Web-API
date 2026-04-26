namespace E_Commerce.Api.DTOs.v1.Catalog.Requests;

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string Currency,
    Guid CategoryId,
    Guid? BrandId = null);
