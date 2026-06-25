using E_Commerce.Application.Common.Models;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Services;
using MediatR;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Handlers;

/// <summary>
/// Handles <see cref="ListBrandsQuery"/> and returns a paginated brand list.
/// </summary>
public sealed class ListBrandsHandler(IBrandQueryService brandQueryService) : IRequestHandler<ListBrandsQuery, PagedList<BrandReadModel>>
{
    public Task<PagedList<BrandReadModel>> Handle(ListBrandsQuery query, CancellationToken cancellationToken)
    {
        return brandQueryService.ListBrandsAsync(query, cancellationToken);
    }
}
