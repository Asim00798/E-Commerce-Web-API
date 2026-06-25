using E_Commerce.Application.Common.Models;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;
using E_Commerce.ReadModel.DbContext;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Services;

public sealed class BrandQueryService(AppReadDbContext dbContext) : IBrandQueryService
{
    public async Task<PagedList<BrandReadModel>> ListBrandsAsync(ListBrandsQuery query, CancellationToken ct)
    {
        var pageNumber = query.Paging.PageNumber <= 0 ? 1 : query.Paging.PageNumber;
        var pageSize = query.Paging.PageSize <= 0 ? 20 : query.Paging.PageSize;

        var sourceQuery = dbContext.Brands.AsNoTracking().OrderBy(x => x.CreatedAt);
        var totalCount = await sourceQuery.CountAsync(ct);
        var items = await sourceQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedList<BrandReadModel>(items, totalCount, pageNumber, pageSize);
    }

    public Task<BrandReadModel?> GetBrandByIdAsync(Guid id, CancellationToken ct)
    {
        return dbContext.Brands.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    }
}
