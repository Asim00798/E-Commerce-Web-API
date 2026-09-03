using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetPublishedProductsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetProductsByBrandAsync(Guid brandId, CancellationToken ct = default);

    Task<IReadOnlyList<Product>> SearchProductsAsync(
    string searchTerm,
    int pageNumber,
    int pageSize,
    CancellationToken ct = default);

    Task<int> GetSearchTotalCountAsync(
        string searchTerm,
        CancellationToken ct = default);
}