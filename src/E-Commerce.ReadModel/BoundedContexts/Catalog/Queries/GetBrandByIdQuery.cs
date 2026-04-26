using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

/// <summary>
/// Query to retrieve a single brand by its unique identifier.
/// </summary>
public sealed record GetBrandByIdQuery(Guid BrandId) : IQuery<BrandReadModel?>;
