using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.Common.Paging;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

/// <summary>
/// Query to retrieve a paginated list of brands.
/// </summary>
public sealed record ListBrandsQuery(PagingRequest Paging) : IQuery<IPagedResult<BrandReadModel>>;
