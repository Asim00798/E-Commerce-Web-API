using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;
using E_Commerce.ReadModel.Common.Paging;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Handlers;

/// <summary>
/// Handles <see cref="ListBrandsQuery"/> and returns a paginated brand list.
/// </summary>
public sealed class ListBrandsHandler : IQueryHandler<ListBrandsQuery, IPagedResult<BrandReadModel>>
{
    public Task<IPagedResult<BrandReadModel>> HandleAsync(ListBrandsQuery query, CancellationToken cancellationToken = default)
    {
        // TODO: Implement paginated query against CatalogReadDbContext
        throw new NotImplementedException();
    }
}
