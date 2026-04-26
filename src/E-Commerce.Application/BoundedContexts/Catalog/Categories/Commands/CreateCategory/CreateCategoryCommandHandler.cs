using MediatR;
using AutoMapper;
using E_Commerce.Domain.Catalog;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.IntegrationEvents;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper,
    IMediator mediator) : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category(request.Name);
        await categoryRepository.AddAsync(category, cancellationToken);
        
        await mediator.Publish(new CategoryCreatedIntegrationEvent(category.Id, category.Name), cancellationToken);

        return mapper.Map<CategoryDto>(category);
    }
}
