using MediatR;
using AutoMapper;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;
using E_Commerce.Application.Common.Exceptions;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper) : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category == null) throw new NotFoundException(nameof(category), request.Id);

        return mapper.Map<CategoryDto>(category);
    }
}
