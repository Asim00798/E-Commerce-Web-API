using E_Commerce.Application.Common.Models;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Services;

public interface IBrandQueryService
{
    Task<PagedList<BrandReadModel>> ListBrandsAsync(ListBrandsQuery query, CancellationToken ct);
    Task<BrandReadModel?> GetBrandByIdAsync(Guid id, CancellationToken ct);
}
