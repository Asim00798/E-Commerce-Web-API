using MediatR;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.Common.Exceptions;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository) : IRequestHandler<UpdateCategoryCommand, Unit>
{
    public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category == null) throw new NotFoundException(nameof(category), request.Id);

        category.Update(request.Name);
        await categoryRepository.UpdateAsync(category, cancellationToken);

        return Unit.Value;
    }
}
