using E_Commerce.Application.Shared.Models;
using E_Commerce.Application.Shared.Stock;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;

namespace E_Commerce.Infrastructure.Catalog.Services;

/// <summary>
/// Stock-owned implementation of the shared stock capability.
/// All stock mutations go through the Product aggregate to enforce invariants.
/// </summary>
public sealed class StockService : IStockService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StockService(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> DecreaseStockAsync(
        Guid productId,
        Guid variantId,
        int quantity,
        CancellationToken ct = default)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(productId, ct);
            if (product is null)
                return Result.Failure("Product not found.");

            product.DecreaseStock(variantId, quantity);

            await _productRepository.UpdateAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> IncreaseStockAsync(
        Guid productId,
        Guid variantId,
        int quantity,
        CancellationToken ct = default)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(productId, ct);
            if (product is null)
                return Result.Failure("Product not found.");

            product.IncreaseStock(variantId, quantity);

            await _productRepository.UpdateAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}