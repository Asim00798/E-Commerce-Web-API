using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.Common.Paging;
using E_Commerce.Application.Common.Models;
using MediatR;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

/// <summary>
/// Query to retrieve a paginated list of brands.
/// </summary>
public sealed record ListBrandsQuery(PagingRequest Paging) : IRequest<PagedList<BrandReadModel>>;
