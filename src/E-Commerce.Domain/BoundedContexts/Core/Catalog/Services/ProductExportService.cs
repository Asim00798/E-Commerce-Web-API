using E_Commerce.Domain.Catalog.AggregateRoots.Product;
using E_Commerce.Domain.Catalog.ValueObjects;
using E_Commerce.Domain.Catalog.Exceptions;
using E_Commerce.Domain.SharedKernel.Interfaces;

namespace E_Commerce.Domain.Catalog.Services
{
    public class ProductExportService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductExportFormatter _exportFormatter; // domain interface for formatting

        public ProductExportService(
            IProductRepository productRepository,
            IProductExportFormatter exportFormatter)
        {
            _productRepository = productRepository;
            _exportFormatter = exportFormatter;
        }

        public async Task<byte[]> ExportProductsAsync(
            ISpecification<Product> specification,
            ExportFormat format,
            CancellationToken cancellationToken = default)
        {
            var products = await _productRepository.FindAsync(specification, cancellationToken);

            if (!products.Any())
                throw new NoProductsToExportException();

            return await _exportFormatter.FormatAsync(products, format, cancellationToken);
        }
    }

    public enum ExportFormat { Csv, Xml, Json }
}
