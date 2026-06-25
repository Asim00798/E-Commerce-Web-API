using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using MediatR;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

/// <summary>
/// Query to retrieve a single product by its unique identifier.
/// </summary>
public sealed record GetProductByIdQuery(Guid ProductId) : IRequest<ProductReadModel?>;
