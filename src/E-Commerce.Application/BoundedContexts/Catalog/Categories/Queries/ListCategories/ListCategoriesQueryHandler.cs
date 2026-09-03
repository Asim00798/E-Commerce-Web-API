using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Queries.ListCategories;

public sealed class ListCategoriesQueryHandler
    : IRequestHandler<ListCategoriesQuery, Result<PagedList<CategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public ListCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<PagedList<CategoryDto>>> Handle(
        ListCategoriesQuery query,
        CancellationToken ct)
    {
        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 20;

        var categories = await _categoryRepository.GetPagedAsync(pageNumber, pageSize, ct);
        var totalCount = await _categoryRepository.GetTotalCountAsync(ct);

        var dtos = categories.Select(category => new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ImageFileIds = category.Images.Select(x => x.FileId).ToList()
        }).ToList();

        var pagedList = new PagedList<CategoryDto>(
            dtos,
            totalCount,
            pageNumber,
            pageSize);

        return Result<PagedList<CategoryDto>>.Success(pagedList);
    }
}