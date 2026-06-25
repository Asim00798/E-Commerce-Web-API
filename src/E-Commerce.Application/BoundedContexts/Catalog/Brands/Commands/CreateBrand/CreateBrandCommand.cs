using MediatR;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.CreateBrand;

public record CreateBrandCommand(string Name, string? Description, string? LogoUrl) : IRequest<BrandDto>;