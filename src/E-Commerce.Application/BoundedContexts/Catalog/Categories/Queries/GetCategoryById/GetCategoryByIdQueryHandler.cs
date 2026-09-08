using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CategoryDto>> Handle(
        GetCategoryByIdQuery query,
        CancellationToken ct)
    {
        var category = await GetCategoryAsync(query.CategoryId, ct);
        if (category is null)
            return Result<CategoryDto>.Failure("Category not found.");

        var dto = MapToDto(category);
        return Result<CategoryDto>.Success(dto);
    }

    private async Task<Category?> GetCategoryAsync(
        Guid categoryId,
        CancellationToken ct)
    {
        return await _categoryRepository.GetByIdAsync(categoryId, ct);
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ImageFileIds = category.Images.Select(x => x.FileId).ToList()
        };
    }
}