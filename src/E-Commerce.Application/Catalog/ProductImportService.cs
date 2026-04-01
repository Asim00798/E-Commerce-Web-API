using E_Commerce.Domain.Catalog.AggregateRoots.Product;
using E_Commerce.Domain.Catalog.AggregateRoots.Brand;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Domain.Catalog.Rules;
using E_Commerce.Domain.Catalog.ValueObjects;
using E_Commerce.Domain.Catalog.Exceptions;
using E_Commerce.Domain.SharedKernel.ValueObjects;

namespace E_Commerce.Application.Catalog
{
    public class ProductImportService
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork; // domain interface for transaction coordination

        public ProductImportService(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task ImportProductAsync(
            ProductImportData data,
            string importedBy,
            CancellationToken cancellationToken = default)
        {
            // Transaction management and logging can be added later as needed.
            // Ensure SKU uniqueness
            var existing = await _productRepository.FindBySkuAsync(data.Sku, cancellationToken);
            if (existing != null)
                throw new DuplicateSkuException(data.Sku);

            // Ensure brand exists
            var brand = await _brandRepository.GetByIdAsync(data.BrandId, cancellationToken)
                ?? throw new BrandNotFoundException(data.BrandId);

            // Create product via factory (could be injected)
            var product = ProductFactory.CreateNew(
                data.Name,
                data.Description,
                data.Sku,
                data.BasePrice,
                brand.Id,
                data.CategoryId,
                importedBy
            );

            // Apply default policies if needed (e.g., set default shipping class)
            product.ApplyDefaultPolicies();

            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            product.AddDomainEvent(new ProductImported(product.Id, importedBy));
        }
    }

    public record ProductImportData(
        string Name,
        string Description,
        string Sku,
        Money BasePrice,
        BrandId BrandId,
        CategoryId? CategoryId);
}
