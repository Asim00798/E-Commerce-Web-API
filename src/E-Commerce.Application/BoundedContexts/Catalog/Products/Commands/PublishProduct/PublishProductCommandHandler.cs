using MediatR;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.Common.Exceptions;
using E_Commerce.Application.BoundedContexts.Catalog.Products.IntegrationEvents;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.PublishProduct;

public class PublishProductCommandHandler(
    IProductRepository productRepository,
    IMediator mediator) : IRequestHandler<PublishProductCommand, Unit>
{
    public async Task<Unit> Handle(PublishProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (product == null)
            throw new NotFoundException(nameof(product), request.Id);

        product.Publish();
        await productRepository.UpdateAsync(product, cancellationToken);

        await mediator.Publish(new ProductPublishedIntegrationEvent(product.Id, product.Name), cancellationToken);

        return Unit.Value;
    }
}
