using AutoMapper;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.MappingProfiles;

public class BrandProfile : Profile
{
    public BrandProfile()
    {
        CreateMap<Brand, BrandDto>();
    }
}
