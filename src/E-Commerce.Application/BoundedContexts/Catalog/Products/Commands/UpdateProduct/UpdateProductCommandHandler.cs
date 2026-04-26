using MediatR;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.Common.Exceptions;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler(
    IProductRepository productRepository) : IRequestHandler<UpdateProductCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (product == null)
            throw new NotFoundException(nameof(product), request.Id);
        
        await productRepository.UpdateAsync(product, cancellationToken);

        return Unit.Value;
    }
}
