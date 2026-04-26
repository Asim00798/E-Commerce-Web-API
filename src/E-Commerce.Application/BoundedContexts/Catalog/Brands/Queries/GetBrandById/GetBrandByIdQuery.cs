using MediatR;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.GetBrandById;

public record GetBrandByIdQuery(Guid Id) : IRequest<BrandDto>;
