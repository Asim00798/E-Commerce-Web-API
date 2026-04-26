using MediatR;
using AutoMapper;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.BoundedContexts.Catalog.Products.DTOs;
using E_Commerce.Application.Common.Models;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Queries.ListProducts;

public class ListProductsQueryHandler(
    IProductRepository productRepository,
    IMapper mapper) : IRequestHandler<ListProductsQuery, PagedList<ProductListDto>>
{
    public async Task<PagedList<ProductListDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("List logic requires pagination implementation on repository.");
    }
}
