using E_Commerce.Application.Common.Models;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;
using E_Commerce.ReadModel.DbContext;
using E_Commerce.ReadModel.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Services;

public sealed class ProductQueryService(AppReadDbContext dbContext, ICacheService cacheService) : IProductQueryService
{
    public async Task<PagedList<ProductListReadModel>> ListProductsAsync(ListProductsQuery query, CancellationToken ct)
    {
        var cacheKey = $"ProductList_{JsonSerializer.Serialize(query)}";

        var cached = await cacheService.GetAsync<PagedList<ProductListReadModel>>(cacheKey, ct);
        if (cached is not null)
        {
            return cached;
        }

        var pageNumber = query.Paging.PageNumber <= 0 ? 1 : query.Paging.PageNumber;
        var pageSize = query.Paging.PageSize <= 0 ? 20 : query.Paging.PageSize;

        var sourceQuery = dbContext.ProductList.AsNoTracking().OrderBy(x => x.CreatedAt);
        var totalCount = await sourceQuery.CountAsync(ct);
        var items = await sourceQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var result = new PagedList<ProductListReadModel>(items, totalCount, pageNumber, pageSize);
        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), ct);

        return result;
    }

    public async Task<ProductReadModel?> GetProductByIdAsync(Guid id, CancellationToken ct)
    {
        var cacheKey = $"Product_{id}";
        var cached = await cacheService.GetAsync<ProductReadModel>(cacheKey, ct);
        if (cached is not null)
        {
            return cached;
        }

        var result = await dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (result is not null)
        {
            await cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1), ct);
        }

        return result;
    }
}
