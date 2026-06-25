using E_Commerce.Application.Common.Models;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Entities;
using E_Commerce.ReadModel.BoundedContexts.Catalog.Queries;

namespace E_Commerce.ReadModel.BoundedContexts.Catalog.Services;

public interface ICategoryQueryService
{
    Task<PagedList<CategoryReadModel>> ListCategoriesAsync(ListCategoriesQuery query, CancellationToken ct);
    Task<CategoryReadModel?> GetCategoryByIdAsync(Guid id, CancellationToken ct);
}
