using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.Common.Paging;
using E_Commerce.Application.Common.Models;
using MediatR;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

/// <summary>
/// Query to retrieve a paginated list of categories.
/// </summary>
public sealed record ListCategoriesQuery(PagingRequest Paging) : IRequest<PagedList<CategoryReadModel>>;
