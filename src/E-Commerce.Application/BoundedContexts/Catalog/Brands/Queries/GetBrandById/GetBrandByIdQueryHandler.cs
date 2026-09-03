using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.GetBrandById;

public sealed class GetBrandByIdQueryHandler
    : IRequestHandler<GetBrandByIdQuery, Result<BrandDto>>
{
    private readonly IBrandRepository _brandRepository;

    public GetBrandByIdQueryHandler(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<Result<BrandDto>> Handle(
        GetBrandByIdQuery query,
        CancellationToken ct)
    {
        var brand = await _brandRepository.GetByIdAsync(query.BrandId, ct);
        if (brand is null)
            return Result<BrandDto>.Failure("Brand not found.");

        var dto = new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            DescriptionText = brand.DescriptionText,
            LogoFileId = brand.Logo.FileId
        };

        return Result<BrandDto>.Success(dto);
    }
}