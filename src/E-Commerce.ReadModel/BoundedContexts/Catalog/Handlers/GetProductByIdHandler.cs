using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Services;
using MediatR;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Handlers;

/// <summary>
/// Handles <see cref="GetProductByIdQuery"/> and returns a <see cref="ProductReadModel"/>.
/// </summary>
public sealed class GetProductByIdHandler(IProductQueryService productQueryService) : IRequestHandler<GetProductByIdQuery, ProductReadModel?>
{
    public Task<ProductReadModel?> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        return productQueryService.GetProductByIdAsync(query.ProductId, cancellationToken);
    }
}
