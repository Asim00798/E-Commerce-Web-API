using MediatR;
using AutoMapper;
using E_Commerce.Domain.Catalog;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;
using E_Commerce.Application.BoundedContexts.Catalog.Products.IntegrationEvents;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IMapper mapper,
    IMediator mediator) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var price = new Money(request.Price, "USD");
        var product = Product.Create(request.Name, price, request.CategoryId, request.BrandId);

        await productRepository.AddAsync(product, cancellationToken);
        
        await mediator.Publish(new ProductCreatedIntegrationEvent(product.Id, product.Name), cancellationToken);

        return mapper.Map<ProductDto>(product);
    }
}
