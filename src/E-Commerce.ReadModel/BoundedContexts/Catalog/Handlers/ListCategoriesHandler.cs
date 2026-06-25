using E_Commerce.Application.Common.Models;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Services;
using MediatR;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Handlers;

/// <summary>
/// Handles <see cref="ListCategoriesQuery"/> and returns a paginated category list.
/// </summary>
public sealed class ListCategoriesHandler(ICategoryQueryService categoryQueryService) : IRequestHandler<ListCategoriesQuery, PagedList<CategoryReadModel>>
{
    public Task<PagedList<CategoryReadModel>> Handle(ListCategoriesQuery query, CancellationToken cancellationToken)
    {
        return categoryQueryService.ListCategoriesAsync(query, cancellationToken);
    }
}
