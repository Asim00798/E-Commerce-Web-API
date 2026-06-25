using MediatR;
using E_Commerce.Application.Common.Models;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.ListBrands;

public class ListBrandsQuery : IRequest<PagedList<BrandDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }      // e.g., "name", "-name" (desc)
}