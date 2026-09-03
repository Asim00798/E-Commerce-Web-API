using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.RemoveCategoryImage;

public sealed class RemoveCategoryImageCommandHandler
    : IRequestHandler<RemoveCategoryImageCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCategoryImageCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RemoveCategoryImageCommand command,
        CancellationToken ct)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(command.CategoryId, ct);
            if (category is null)
                return Result.Failure("Category not found.");

            category.RemoveImage(command.FileId);

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