using MediatR;
using AutoMapper;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.ListCategories;

public class ListCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper) : IRequestHandler<ListCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(ListCategoriesQuery request, CancellationToken cancellationToken)
    {
        // Assuming repository has a way to list all
        var categories = new List<E_Commerce.Domain.Catalog.Category>(); 
        return mapper.Map<List<CategoryDto>>(categories);
    }
}
