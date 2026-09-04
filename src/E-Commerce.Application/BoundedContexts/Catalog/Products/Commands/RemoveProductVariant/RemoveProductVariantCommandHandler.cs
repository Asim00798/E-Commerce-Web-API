using E_Commerce.Application.Shared.Caching;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.RemoveProductVariant;

public sealed class RemoveProductVariantCommandHandler : IRequestHandler<RemoveProductVariantCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICache _cache;
    public RemoveProductVariantCommandHandler(IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ICache cache)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result> Handle(RemoveProductVariantCommand command, CancellationToken ct)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
            if (product is null) return Result.Failure("Product not found.");

            product.RemoveVariant(command.VariantId);

            await _productRepository.UpdateAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Invalidate cache after successful commit
            await _cache.RemoveAsync($"catalog:product:{command.ProductId}", ct);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}