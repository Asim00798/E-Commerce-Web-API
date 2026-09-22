using E_Commerce.Application.Shared.Models;

namespace E_Commerce.Application.Shared.Stock;

/// <summary>
/// Cross-context stock capability owned by Catalog.
/// Ordering uses this contract to decrease/increase stock synchronously.
/// If an Inventory context is introduced later, it can implement this same contract.
/// </summary>
public interface IStockService
{
    Task<Result> DecreaseStockAsync(
        Guid productId,
        Guid variantId,
        int quantity,
        CancellationToken ct = default);

    Task<Result> IncreaseStockAsync(
        Guid productId,
        Guid variantId,
        int quantity,
        CancellationToken ct = default);
}