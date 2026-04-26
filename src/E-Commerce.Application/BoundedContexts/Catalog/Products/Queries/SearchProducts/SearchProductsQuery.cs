using MediatR;
using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.SearchProducts;

public record SearchProductsQuery(string SearchTerm, int PageNumber = 1, int PageSize = 10) : IRequest<List<ProductListDto>>;
