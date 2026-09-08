using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Entities;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await GetProductAsync(query.ProductId, ct);
        if (product is null)
            return Result<ProductDto>.Failure("Product not found.");

        var dto = MapToDto(product);
        return Result<ProductDto>.Success(dto);
    }

    private async Task<Product?> GetProductAsync(Guid productId, CancellationToken ct)
    {
        return await _productRepository.GetByIdAsync(productId, ct);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Description = MapDescription(product.Description),
            BrandId = product.BrandId,
            CategoryId = product.CategoryId,
            Status = product.Status.ToString(),
            Tags = product.Tags.ToList(),
            Images = MapImages(product.Images),
            Variants = MapVariants(product.Variants)
        };
    }

    private static ProductDescriptionDto MapDescription(ProductDescription description)
    {
        return new ProductDescriptionDto
        {
            Name = description.Name,
            ShortDescription = description.ShortDescription,
            LongDescription = description.LongDescription,
            Dimensions = description.Dimensions?.ToString(),
            Weight = description.Weight?.ToString(),
            DateOfManufacture = description.DateOfManufacture,
            DateOfExpiry = description.DateOfExpiry,
            Material = description.Material,
            Color = description.Color
        };
    }

    private static List<ProductImageDto> MapImages(IEnumerable<ProductImage> images)
    {
        return images.Select(img => new ProductImageDto
        {
            Id = img.Id,
            FileId = img.FileId,
            AltText = img.AltText,
            IsMain = img.IsMain
        }).ToList();
    }

    private static List<ProductVariantDto> MapVariants(IEnumerable<ProductVariant> variants)
    {
        return variants.Select(v => new ProductVariantDto
        {
            Id = v.Id,
            Name = v.Name,
            Sku = v.SKU,
            PriceAmount = v.Price.Amount,
            Currency = v.Price.Currency,
            StockQuantity = v.StockQuantity
        }).ToList();
    }
}