using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;
using E_Commerce.ReadModel.Common.Paging;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Handlers;

/// <summary>
/// Handles <see cref="ListProductsQuery"/> and returns a paginated product list.
/// </summary>
public sealed class ListProductsHandler : IQueryHandler<ListProductsQuery, IPagedResult<ProductListReadModel>>
{
    public Task<IPagedResult<ProductListReadModel>> HandleAsync(ListProductsQuery query, CancellationToken cancellationToken = default)
    {
        // TODO: Implement paginated query against CatalogReadDbContext
        throw new NotImplementedException();
    }
}
