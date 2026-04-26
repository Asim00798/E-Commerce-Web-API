using MediatR;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string Name) : IRequest<CategoryDto>;
