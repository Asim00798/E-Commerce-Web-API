using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateProductCommand command,
        CancellationToken ct)
    {
        try
        {
            Dimension? dimensions = ParseDimension(command.Dimensions);
            Weight? weight = ParseWeight(command.Weight);

            var description = new ProductDescription(
                command.Name,
                command.ShortDescription,
                command.LongDescription,
                dimensions,
                weight,
                command.DateOfManufacture,
                command.DateOfExpiry,
                command.Material,
                command.Color);

            var product = Product.Create(
                description,
                command.BrandId,
                command.CategoryId,
                command.Tags);

            await _productRepository.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<Guid>.Success(product.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }

    private static Dimension? ParseDimension(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var parts = value.Split('x', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) throw new BusinessRuleViolationException("Dimensions must be L x W x H.");

        if (!decimal.TryParse(parts[0], out var length) ||
            !decimal.TryParse(parts[1], out var width) ||
            !decimal.TryParse(parts[2], out var height))
            throw new BusinessRuleViolationException("Invalid dimension values.");

        return new Dimension(length, width, height);
    }

    private static Weight? ParseWeight(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var text = value.Replace("kg", "", StringComparison.OrdinalIgnoreCase).Trim();
        if (!decimal.TryParse(text, out var kilograms))
            throw new BusinessRuleViolationException("Invalid weight value.");

        return new Weight(kilograms);
    }
}