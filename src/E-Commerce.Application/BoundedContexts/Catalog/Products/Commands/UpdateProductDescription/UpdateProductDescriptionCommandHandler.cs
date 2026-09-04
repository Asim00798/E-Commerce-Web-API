using E_Commerce.Application.Shared.Caching;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.UpdateProductDescription;

public sealed class UpdateProductDescriptionCommandHandler
    : IRequestHandler<UpdateProductDescriptionCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICache _cache;

    public UpdateProductDescriptionCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICache cache)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result> Handle(
        UpdateProductDescriptionCommand command,
        CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        if (product is null)
            return Result.Failure("Product not found.");

        // Build the new description using the existing immutable record's With methods.
        // This preserves unchanged complex fields (Dimensions, Weight, Dates) while updating the provided ones.
        var newDescription = product.Description
            .WithName(command.Name)
            .WithShortDescription(command.ShortDescription)
            .WithLongDescription(command.LongDescription)
            .WithMaterial(command.Material)
            .WithColor(command.Color);

        // Assumes the Product aggregate exposes an UpdateDescription method.
        product.UpdateDescription(newDescription);

        await _productRepository.UpdateAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache after successful commit
        await _cache.RemoveAsync($"catalog:product:{command.ProductId}", ct);

        return Result.Success();
    }
}