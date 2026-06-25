using AutoMapper;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;

using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.ListBrands;

public class ListBrandsQueryHandler(
    IBrandRepository brandRepository,
    IMapper mapper) : IRequestHandler<ListBrandsQuery, List<BrandDto>>
{
    public async Task<List<BrandDto>> Handle(ListBrandsQuery request, CancellationToken cancellationToken)
    {
        // Assuming repository has a way to list all
        var brands = new List<E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors.Brand>(); 
        return mapper.Map<List<BrandDto>>(brands);
    }
}
