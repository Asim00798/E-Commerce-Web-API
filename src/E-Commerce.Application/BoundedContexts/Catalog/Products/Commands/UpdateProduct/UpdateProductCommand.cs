using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProduct;

public record UpdateProductCommand(Guid Id, string Name, decimal Price, Guid CategoryId, Guid? BrandId) : IRequest<Unit>;
