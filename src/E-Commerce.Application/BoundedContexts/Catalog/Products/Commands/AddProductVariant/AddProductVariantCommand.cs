using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.AddProductVariant;

public record AddProductVariantCommand(Guid ProductId, string Name, string? Sku, decimal Price, int StockQuantity) : IRequest<Guid>;
