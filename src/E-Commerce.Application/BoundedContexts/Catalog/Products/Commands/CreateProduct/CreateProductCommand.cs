using MediatR;
using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;
using E_Commerce.Application.Common.Security;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.CreateProduct;

[Authorize(Roles = "Admin,Manager")]
public record CreateProductCommand(string Name, decimal Price, Guid CategoryId, Guid? BrandId) : IRequest<ProductDto>;
