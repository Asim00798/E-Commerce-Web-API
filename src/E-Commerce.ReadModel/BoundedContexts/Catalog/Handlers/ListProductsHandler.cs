using E_Commerce.Application.Common.Models;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Services;
using MediatR;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Handlers;

/// <summary>
/// Handles <see cref="ListProductsQuery"/> and returns a paginated product list.
/// </summary>
public sealed class ListProductsHandler(IProductQueryService productQueryService) : IRequestHandler<ListProductsQuery, PagedList<ProductListReadModel>>
{
    public Task<PagedList<ProductListReadModel>> Handle(ListProductsQuery query, CancellationToken cancellationToken)
    {
        return productQueryService.ListProductsAsync(query, cancellationToken);
    }
}
