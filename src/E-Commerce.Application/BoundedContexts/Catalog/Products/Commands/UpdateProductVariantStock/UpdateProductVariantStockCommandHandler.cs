using MediatR;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.Common.Exceptions;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProductVariantStock;

public class UpdateProductVariantStockCommandHandler(
    IProductRepository productRepository) : IRequestHandler<UpdateProductVariantStockCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductVariantStockCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        
        if (product == null)
            throw new NotFoundException(nameof(product), request.ProductId);

        var variant = product.Variants.FirstOrDefault(v => v.Id == request.VariantId);
        if (variant == null)
            throw new NotFoundException(nameof(variant), request.VariantId);
        
        // Example implementation for updating stock if the method is available on variant
        // variant.AdjustStock(request.QuantityDelta);

        await productRepository.UpdateAsync(product, cancellationToken);

        return Unit.Value;
    }
}
