using E_Commerce.Application.Common.Models;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Services;

public interface IProductQueryService
{
    Task<PagedList<ProductListReadModel>> ListProductsAsync(ListProductsQuery query, CancellationToken ct);
    Task<ProductReadModel?> GetProductByIdAsync(Guid id, CancellationToken ct);
}
