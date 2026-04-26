namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;

public record CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
