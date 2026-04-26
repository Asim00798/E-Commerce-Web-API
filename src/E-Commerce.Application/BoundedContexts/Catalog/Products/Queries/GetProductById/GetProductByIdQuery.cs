using MediatR;
using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;
