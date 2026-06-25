using E_Commerce.Application.Common.Models;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;
using E_Commerce.ReadModel.DbContext;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Services;

public sealed class CategoryQueryService(AppReadDbContext dbContext) : ICategoryQueryService
{
    public async Task<PagedList<CategoryReadModel>> ListCategoriesAsync(ListCategoriesQuery query, CancellationToken ct)
    {
        var pageNumber = query.Paging.PageNumber <= 0 ? 1 : query.Paging.PageNumber;
        var pageSize = query.Paging.PageSize <= 0 ? 20 : query.Paging.PageSize;

        var sourceQuery = dbContext.Categories.AsNoTracking().OrderBy(x => x.CreatedAt);
        var totalCount = await sourceQuery.CountAsync(ct);
        var items = await sourceQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedList<CategoryReadModel>(items, totalCount, pageNumber, pageSize);
    }

    public Task<CategoryReadModel?> GetCategoryByIdAsync(Guid id, CancellationToken ct)
    {
        return dbContext.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
