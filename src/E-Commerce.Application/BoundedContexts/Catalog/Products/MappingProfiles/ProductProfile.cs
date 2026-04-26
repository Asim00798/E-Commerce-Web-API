using AutoMapper;
using E_Commerce.Domain.Catalog;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.MappingProfiles;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency));

        CreateMap<Product, ProductListDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency));

        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency));
    }
}
