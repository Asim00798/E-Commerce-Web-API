using MediatR;
using AutoMapper;
using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;
using E_Commerce.Application.BoundedContexts.Catalog.Services;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.SearchProducts;

public class SearchProductsQueryHandler(
    CatalogSearchService searchService,
    IMapper mapper) : IRequestHandler<SearchProductsQuery, List<ProductListDto>>
{
    public async Task<List<ProductListDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await searchService.SearchAsync(request.SearchTerm, cancellationToken);
        return mapper.Map<List<ProductListDto>>(products);
    }
}
