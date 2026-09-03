using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;
using E_Commerce.Application.Shared.Models;
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
        var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 20;

        var brands = await _brandRepository.GetPagedAsync(pageNumber, pageSize, ct);
        var totalCount = await _brandRepository.GetTotalCountAsync(ct);

        var dtos = brands.Select(brand => new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            DescriptionText = brand.DescriptionText,
            LogoFileId = brand.Logo.FileId
        }).ToList();

        var pagedList = new PagedList<BrandDto>(
            dtos,
            totalCount,
            pageNumber,
            pageSize);

        return Result<PagedList<BrandDto>>.Success(pagedList);
    }
}