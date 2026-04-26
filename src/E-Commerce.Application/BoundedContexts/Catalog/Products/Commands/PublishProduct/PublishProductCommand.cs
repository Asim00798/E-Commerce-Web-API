using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.PublishProduct;

public record PublishProductCommand(Guid Id) : IRequest<Unit>;
