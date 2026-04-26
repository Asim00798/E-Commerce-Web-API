using MediatR;
using AutoMapper;
using E_Commerce.Domain.Catalog;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.IntegrationEvents;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.CreateBrand;

public class CreateBrandCommandHandler(
    IBrandRepository brandRepository,
    IMapper mapper,
    IMediator mediator) : IRequestHandler<CreateBrandCommand, BrandDto>
{
    public async Task<BrandDto> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = new Brand(request.Name);
        await brandRepository.AddAsync(brand, cancellationToken);
        
        await mediator.Publish(new BrandCreatedIntegrationEvent(brand.Id, brand.Name), cancellationToken);

        return mapper.Map<BrandDto>(brand);
    }
}
