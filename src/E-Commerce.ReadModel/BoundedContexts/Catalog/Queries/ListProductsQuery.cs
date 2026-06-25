using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.Common.Paging;
using E_Commerce.Application.Common.Models;
using MediatR;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

/// <summary>
/// Query to retrieve a paginated list of products with optional filtering and sorting.
/// </summary>
public sealed record ListProductsQuery(PagingRequest Paging) : IRequest<PagedList<ProductListReadModel>>;
