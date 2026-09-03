using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateCategoryCommand command,
        CancellationToken ct)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(command.CategoryId, ct);
            if (category is null)
                return Result.Failure("Category not found.");

            if (!string.IsNullOrWhiteSpace(command.Name))
                category.UpdateName(command.Name);

            if (!string.IsNullOrWhiteSpace(command.Description))
                category.UpdateDescription(command.Description);

            if (command.ClearParent)
            {
                category.ClearParent();
            }
            else if (command.ParentCategoryId.HasValue)
            {
                category.AssignParent(command.ParentCategoryId.Value);
            }

            await _categoryRepository.UpdateAsync(category, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}