using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Services;
using MediatR;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Handlers;

/// <summary>
/// Handles <see cref="GetCategoryByIdQuery"/> and returns a <see cref="CategoryReadModel"/>.
/// </summary>
public sealed class GetCategoryByIdHandler(ICategoryQueryService categoryQueryService) : IRequestHandler<GetCategoryByIdQuery, CategoryReadModel?>
{
    public Task<CategoryReadModel?> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        return categoryQueryService.GetCategoryByIdAsync(query.CategoryId, cancellationToken);
    }
}
