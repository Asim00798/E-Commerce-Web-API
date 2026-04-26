using E_Commerce.Domain.Catalog;
using E_Commerce.Domain.Catalog.Repositories;

namespace E_Commerce.Application.BoundedContexts.Catalog.Services;

public class CatalogSearchService(IProductRepository productRepository)
{
    public async Task<List<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken)
    {
        return new List<Product>();
    }
}
