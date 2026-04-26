using E_Commerce.ReadModel.Abstractions;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.Common.Paging;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

/// <summary>
/// Query to retrieve a paginated list of categories.
/// </summary>
public sealed record ListCategoriesQuery(PagingRequest Paging) : IQuery<IPagedResult<CategoryReadModel>>;
