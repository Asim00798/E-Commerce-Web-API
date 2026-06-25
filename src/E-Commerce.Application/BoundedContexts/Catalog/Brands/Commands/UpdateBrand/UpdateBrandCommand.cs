using MediatR;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.UpdateBrand;

public record UpdateBrandCommand(Guid Id, string Name, string? Description, string? LogoUrl) : IRequest<BrandDto>;