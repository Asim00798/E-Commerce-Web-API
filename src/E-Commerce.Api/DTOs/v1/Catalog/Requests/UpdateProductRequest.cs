namespace E_Commerce.Api.DTOs.v1.Catalog.Requests;

public record UpdateProductRequest(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency);
