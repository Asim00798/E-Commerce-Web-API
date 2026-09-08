using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Category.Behaviors;
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
        var paging = NormalizePaging(query.PageNumber, query.PageSize);

        var categories = await _categoryRepository.GetPagedAsync(
            paging.PageNumber,
            paging.PageSize,
            ct);

        var totalCount = await _categoryRepository.GetTotalCountAsync(ct);

        var dtos = categories.Select(MapToDto).ToList();
        var pagedList = BuildPagedList(dtos, totalCount, paging);

        return Result<PagedList<CategoryDto>>.Success(pagedList);
    }

    private static (int PageNumber, int PageSize) NormalizePaging(
        int pageNumber,
        int pageSize)
    {
        var normalizedPageNumber = pageNumber > 0 ? pageNumber : 1;
        var normalizedPageSize = pageSize > 0 ? pageSize : 20;

        return (normalizedPageNumber, normalizedPageSize);
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

    private static PagedList<CategoryDto> BuildPagedList(
        IReadOnlyList<CategoryDto> dtos,
        int totalCount,
        (int PageNumber, int PageSize) paging)
    {
        return new PagedList<CategoryDto>(
            dtos,
            totalCount,
            paging.PageNumber,
            paging.PageSize);
    }
}