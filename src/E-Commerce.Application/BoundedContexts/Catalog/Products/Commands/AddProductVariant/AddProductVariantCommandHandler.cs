using MediatR;
using E_Commerce.Domain.Catalog;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using E_Commerce.Application.Common.Exceptions;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.AddProductVariant;

public class AddProductVariantCommandHandler(
    IProductRepository productRepository) : IRequestHandler<AddProductVariantCommand, Guid>
{
    public async Task<Guid> Handle(AddProductVariantCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        
        if (product == null)
            throw new NotFoundException(nameof(product), request.ProductId);

        var variant = new ProductVariant(request.ProductId, request.Name, request.Sku, new Money(request.Price, "USD"), request.StockQuantity);
        
        product.AddVariant(variant);
        await productRepository.UpdateAsync(product, cancellationToken);

        return variant.Id;
    }
}
