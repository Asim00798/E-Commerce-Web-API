using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.AddProductVariant;

public sealed class AddProductVariantCommandHandler : IRequestHandler<AddProductVariantCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddProductVariantCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AddProductVariantCommand command, CancellationToken ct)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
            if (product is null) return Result<Guid>.Failure("Product not found.");

            var money = new Money(command.PriceAmount, command.Currency);
            product.AddVariant(command.Name, command.Sku, money, command.StockQuantity);

            await _productRepository.UpdateAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var variantId = product.Variants.Last().Id;
            return Result<Guid>.Success(variantId);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}