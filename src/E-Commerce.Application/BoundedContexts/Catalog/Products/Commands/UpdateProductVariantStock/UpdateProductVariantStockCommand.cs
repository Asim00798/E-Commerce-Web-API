using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProductVariantStock;

public record UpdateProductVariantStockCommand(Guid ProductId, Guid VariantId, int QuantityDelta) : IRequest<Unit>;
