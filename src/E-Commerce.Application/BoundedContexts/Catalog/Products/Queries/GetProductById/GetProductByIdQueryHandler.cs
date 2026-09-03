using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors;
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
        var product = await _productRepository.GetByIdAsync(query.ProductId, ct);
        if (product is null) return Result<ProductDto>.Failure("Product not found.");

        var dto = MapToDto(product);
        return Result<ProductDto>.Success(dto);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Description = new ProductDescriptionDto
            {
                Name = product.Description.Name,
                ShortDescription = product.Description.ShortDescription,
                LongDescription = product.Description.LongDescription,
                Dimensions = product.Description.Dimensions?.ToString(),
                Weight = product.Description.Weight?.ToString(),
                DateOfManufacture = product.Description.DateOfManufacture,
                DateOfExpiry = product.Description.DateOfExpiry,
                Material = product.Description.Material,
                Color = product.Description.Color
            },
            BrandId = product.BrandId,
            CategoryId = product.CategoryId,
            Status = product.Status.ToString(),
            Tags = product.Tags.ToList(),
            Images = product.Images.Select(img => new ProductImageDto
            {
                Id = img.Id,
                FileId = img.FileId,
                AltText = img.AltText,
                IsMain = img.IsMain
            }).ToList(),
            Variants = product.Variants.Select(v => new ProductVariantDto
            {
                Id = v.Id,
                Name = v.Name,
                Sku = v.SKU,
                PriceAmount = v.Price.Amount,
                Currency = v.Price.Currency,
                StockQuantity = v.StockQuantity
            }).ToList()
        };
    }
}