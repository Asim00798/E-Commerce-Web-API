using MediatR;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;
