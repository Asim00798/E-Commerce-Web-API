using AutoMapper;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;
using E_Commerce.Application.Common.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Queries.GetBrandById;

public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, BrandDto>
{
    private readonly IBrandRepository _repository;
    private readonly IMapper _mapper;

    public GetBrandByIdQueryHandler(IBrandRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<BrandDto> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (brand == null)
            throw new NotFoundException(nameof(Brand), request.Id);

        return _mapper.Map<BrandDto>(brand);
    }
}