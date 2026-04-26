using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Handlers;

/// <summary>
/// Handles <see cref="GetCategoryByIdQuery"/> and returns a <see cref="CategoryReadModel"/>.
/// </summary>
public sealed class GetCategoryByIdHandler : IQueryHandler<GetCategoryByIdQuery, CategoryReadModel?>
{
    public Task<CategoryReadModel?> HandleAsync(GetCategoryByIdQuery query, CancellationToken cancellationToken = default)
    {
        // TODO: Implement query against CatalogReadDbContext
        throw new NotImplementedException();
    }
}
