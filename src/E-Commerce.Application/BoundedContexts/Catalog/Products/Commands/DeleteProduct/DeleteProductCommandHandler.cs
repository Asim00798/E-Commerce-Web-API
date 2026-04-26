using MediatR;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.Common.Exceptions;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler(
    IProductRepository productRepository) : IRequestHandler<DeleteProductCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (product == null)
            throw new NotFoundException(nameof(product), request.Id);

        await productRepository.DeleteAsync(product, cancellationToken);

        return Unit.Value;
    }
}
