using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Product.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.SearchProducts;

public sealed class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, Result<PagedList<ProductListDto>>>
{
    private readonly IProductRepository _productRepository;

    public SearchProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PagedList<ProductListDto>>> Handle(SearchProductsQuery query, CancellationToken ct)
    {
        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 20;

        // Requires IProductRepository.SearchProductsAsync
        var products = await _productRepository.SearchProductsAsync(
            query.SearchTerm,
            pageNumber,
            pageSize,
            ct);

        var totalCount = await _productRepository.GetSearchTotalCountAsync(query.SearchTerm, ct);

        var dtos = products.Select(MapToListDto).ToList();

        var pagedList = new PagedList<ProductListDto>(dtos, totalCount, pageNumber, pageSize);
        return Result<PagedList<ProductListDto>>.Success(pagedList);
    }

    private static ProductListDto MapToListDto(Product product)
    {
        return new ProductListDto
        {
            Id = product.Id,
            Name = product.Description.Name,
            ShortDescription = product.Description.ShortDescription,
            BrandId = product.BrandId,
            CategoryId = product.CategoryId,
            Status = product.Status.ToString(),
            MinPrice = product.Variants.Any() ? product.Variants.Min(v => v.Price.Amount) : 0,
            Currency = product.Variants.FirstOrDefault()?.Price.Currency ?? string.Empty,
            TotalStock = product.Variants.Sum(v => v.StockQuantity)
        };
    }
}