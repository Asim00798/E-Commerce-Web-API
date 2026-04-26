using MediatR;
using AutoMapper;
using E_Commerce.Domain.Catalog.Repositories;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;
using E_Commerce.Application.Common.Exceptions;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.GetBrandById;

public class GetBrandByIdQueryHandler(
    IBrandRepository brandRepository,
    IMapper mapper) : IRequestHandler<GetBrandByIdQuery, BrandDto>
{
    public async Task<BrandDto> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await brandRepository.GetByIdAsync(request.Id, cancellationToken);
        if (brand == null) throw new NotFoundException(nameof(brand), request.Id);

        return mapper.Map<BrandDto>(brand);
    }
}
