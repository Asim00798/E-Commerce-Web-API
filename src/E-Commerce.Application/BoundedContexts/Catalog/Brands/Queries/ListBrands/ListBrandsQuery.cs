using MediatR;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.ListBrands;

public record ListBrandsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<List<BrandDto>>;
