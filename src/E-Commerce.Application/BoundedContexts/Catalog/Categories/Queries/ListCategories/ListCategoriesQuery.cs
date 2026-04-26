using MediatR;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.ListCategories;

public record ListCategoriesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<List<CategoryDto>>;
