using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;
using E_Commerce.ReadModel.Common.Paging;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Handlers;

/// <summary>
/// Handles <see cref="ListCategoriesQuery"/> and returns a paginated category list.
/// </summary>
public sealed class ListCategoriesHandler : IQueryHandler<ListCategoriesQuery, IPagedResult<CategoryReadModel>>
{
    public Task<IPagedResult<CategoryReadModel>> HandleAsync(ListCategoriesQuery query, CancellationToken cancellationToken = default)
    {
        // TODO: Implement paginated query against CatalogReadDbContext
        throw new NotImplementedException();
    }
}
