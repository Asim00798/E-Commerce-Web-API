using E_Commerce.Application.Shared.Caching;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProductStock;

public sealed class UpdateProductStockCommandHandler
    : IRequestHandler<UpdateProductStockCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICache _cache;

    public UpdateProductStockCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICache cache)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result> Handle(
        UpdateProductStockCommand command,
        CancellationToken ct)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
            if (product is null)
                return Result.Failure("Product not found.");

            var result = AdjustVariantStock(product, command);
            if (!result.Succeeded)
                return result;

            await _productRepository.UpdateAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Invalidate cache after successful commit
            await _cache.RemoveAsync($"catalog:product:{command.ProductId}", ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while updating product stock: {ex.Message}");
        }
    }

    private static Result AdjustVariantStock(
    Product product,
    UpdateProductStockCommand command)
    {
        var variant = product.Variants.FirstOrDefault(v => v.Id == command.ProductVariantId);
        if (variant is null)
            return Result.Failure("Variant not found.");

        int difference = command.NewStockQuantity - variant.StockQuantity;
        ApplyStockDifference(product, command.ProductVariantId, difference);

        return Result.Success();
    }

    private static void ApplyStockDifference(
        Product product,
        Guid productVariantId,
        int difference)
    {
        if (difference > 0)
        {
            product.IncreaseStock(productVariantId, difference);
        }
        else if (difference < 0)
        {
            product.DecreaseStock(productVariantId, -difference);
        }
        // if difference == 0, no change
    }
}