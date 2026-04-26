using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<Unit>;
