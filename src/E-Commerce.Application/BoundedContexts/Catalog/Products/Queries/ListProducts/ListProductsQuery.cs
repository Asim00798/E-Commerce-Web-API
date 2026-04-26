using MediatR;
using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;
using E_Commerce.Application.Common.Models;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.ListProducts;

public record ListProductsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedList<ProductListDto>>;
