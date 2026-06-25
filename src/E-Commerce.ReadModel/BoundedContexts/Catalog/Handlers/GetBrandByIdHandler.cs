using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Services;
using MediatR;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Handlers;

/// <summary>
/// Handles <see cref="GetBrandByIdQuery"/> and returns a <see cref="BrandReadModel"/>.
/// </summary>
public sealed class GetBrandByIdHandler(IBrandQueryService brandQueryService) : IRequestHandler<GetBrandByIdQuery, BrandReadModel?>
{
    public Task<BrandReadModel?> Handle(GetBrandByIdQuery query, CancellationToken cancellationToken)
    {
        return brandQueryService.GetBrandByIdAsync(query.BrandId, cancellationToken);
    }
}
