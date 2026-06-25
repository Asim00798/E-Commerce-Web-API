using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using MediatR;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

/// <summary>
/// Query to retrieve a single brand by its unique identifier.
/// </summary>
public sealed record GetBrandByIdQuery(Guid BrandId) : IRequest<BrandReadModel?>;
