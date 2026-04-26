using E_Commerce.Domain.Catalog;
using E_Commerce.Domain.Catalog.Repositories;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Services;

public class ProductPublishingService(
    IBrandRepository brandRepository,
    ICategoryRepository categoryRepository)
{
    public async Task<bool> CanPublishAsync(Product product, CancellationToken ct = default)
    {
        // 1. Verify Category is active
        var category = await categoryRepository.GetByIdAsync(product.CategoryId, ct);
        if (category == null) return false;

        // 2. Verify Brand is active (if it exists)
        if (product.BrandId.HasValue)
        {
            var brand = await brandRepository.GetByIdAsync(product.BrandId.Value, ct);
            if (brand == null) return false;
        }

        return true;
    }
}
