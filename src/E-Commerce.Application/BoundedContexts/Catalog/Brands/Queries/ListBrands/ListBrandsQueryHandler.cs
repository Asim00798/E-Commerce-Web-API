using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.ListBrands;

public sealed class ListBrandsQueryHandler
    : IRequestHandler<ListBrandsQuery, Result<PagedList<BrandDto>>>
{
    private readonly IBrandRepository _brandRepository;

    public ListBrandsQueryHandler(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<Result<PagedList<BrandDto>>> Handle(
        ListBrandsQuery query,
        CancellationToken ct)
    {
        var paging = NormalizePaging(query.PageNumber, query.PageSize);

        var brands = await _brandRepository.GetPagedAsync(paging.PageNumber, paging.PageSize, ct);
        var totalCount = await _brandRepository.GetTotalCountAsync(ct);

        var dtos = brands.Select(MapToDto).ToList();
        var pagedList = BuildPagedList(dtos, totalCount, paging);

        return Result<PagedList<BrandDto>>.Success(pagedList);
    }

    private static (int PageNumber, int PageSize) NormalizePaging(
        int pageNumber,
        int pageSize)
    {
        var normalizedPageNumber = pageNumber > 0 ? pageNumber : 1;
        var normalizedPageSize = pageSize > 0 ? pageSize : 20;

        return (normalizedPageNumber, normalizedPageSize);
    }

    private static BrandDto MapToDto(Brand brand)
    {
        return new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            DescriptionText = brand.DescriptionText,
            LogoFileId = brand.Logo.FileId
        };
    }

    private static PagedList<BrandDto> BuildPagedList(
        IReadOnlyList<BrandDto> dtos,
        int totalCount,
        (int PageNumber, int PageSize) paging)
    {
        return new PagedList<BrandDto>(
            dtos,
            totalCount,
            paging.PageNumber,
            paging.PageSize);
    }
}