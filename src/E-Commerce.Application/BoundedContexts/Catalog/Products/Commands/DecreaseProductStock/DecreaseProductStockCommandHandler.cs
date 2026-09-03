using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.DecreaseProductStock;

public sealed class DecreaseProductStockCommandHandler : IRequestHandler<DecreaseProductStockCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DecreaseProductStockCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DecreaseProductStockCommand command, CancellationToken ct)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
            if (product is null) return Result.Failure("Product not found.");

            product.DecreaseStock(command.VariantId, command.Quantity);

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