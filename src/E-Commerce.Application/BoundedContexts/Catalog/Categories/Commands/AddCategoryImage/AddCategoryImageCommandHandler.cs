using E_Commerce.Application.BoundedContexts.Catalog.Categories.Validation;
using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Commands.AddCategoryImage;

public sealed class AddCategoryImageCommandHandler
    : IRequestHandler<AddCategoryImageCommand, Result<Guid>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFileService _fileService;
    private readonly CategoryImageFileValidator _imageValidator;
    private readonly IUnitOfWork _unitOfWork;

    public AddCategoryImageCommandHandler(
        ICategoryRepository categoryRepository,
        IFileService fileService,
        CategoryImageFileValidator imageValidator,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _fileService = fileService;
        _imageValidator = imageValidator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        AddCategoryImageCommand command,
        CancellationToken ct)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(command.CategoryId, ct);
            if (category is null)
                return Result<Guid>.Failure("Category not found.");

            var validation = await _imageValidator.ValidateAsync(command.Image, ct);
            if (!validation.Succeeded)
                return Result<Guid>.Failure(validation.Errors);

            var fileId = await _fileService.UploadAsync(
                command.Image.Content,
                command.Image.FileName,
                command.Image.ContentType,
                ct);

            category.AddImage(fileId, command.Image.FileName);

            await _categoryRepository.UpdateAsync(category, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<Guid>.Success(fileId);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}