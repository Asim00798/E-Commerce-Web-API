using MediatR;
using AutoMapper;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.ListBrands;

public class ListBrandsQueryHandler(
    IBrandRepository brandRepository,
    IMapper mapper) : IRequestHandler<ListBrandsQuery, List<BrandDto>>
{
    public async Task<List<BrandDto>> Handle(ListBrandsQuery request, CancellationToken cancellationToken)
    {
        // Assuming repository has a way to list all
        var brands = new List<E_Commerce.Domain.Catalog.Brand>(); 
        return mapper.Map<List<BrandDto>>(brands);
    }
}
