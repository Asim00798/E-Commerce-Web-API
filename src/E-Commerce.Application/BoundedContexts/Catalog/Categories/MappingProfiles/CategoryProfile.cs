using AutoMapper;
using E_Commerce.Domain.Catalog;
using E_Commerce.Application.BoundedContexts.Catalog.Categories.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.MappingProfiles;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>();
    }
}
