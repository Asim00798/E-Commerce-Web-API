using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Handlers;

/// <summary>
/// Handles <see cref="GetProductByIdQuery"/> and returns a <see cref="ProductReadModel"/>.
/// </summary>
public sealed class GetProductByIdHandler : IQueryHandler<GetProductByIdQuery, ProductReadModel?>
{
    public Task<ProductReadModel?> HandleAsync(GetProductByIdQuery query, CancellationToken cancellationToken = default)
    {
        // TODO: Implement query against CatalogReadDbContext
        throw new NotImplementedException();
    }
}
