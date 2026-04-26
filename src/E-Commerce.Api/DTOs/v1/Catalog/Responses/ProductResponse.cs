namespace E_Commerce.Api.DTOs.v1.Catalog.Responses;

public record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    string Status);
