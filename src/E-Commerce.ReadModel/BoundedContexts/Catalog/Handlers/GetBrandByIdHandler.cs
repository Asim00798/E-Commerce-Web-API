using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Handlers;

/// <summary>
/// Handles <see cref="GetBrandByIdQuery"/> and returns a <see cref="BrandReadModel"/>.
/// </summary>
public sealed class GetBrandByIdHandler : IQueryHandler<GetBrandByIdQuery, BrandReadModel?>
{
    public Task<BrandReadModel?> HandleAsync(GetBrandByIdQuery query, CancellationToken cancellationToken = default)
    {
        // TODO: Implement query against CatalogReadDbContext
        throw new NotImplementedException();
    }
}
