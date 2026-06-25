using AutoMapper;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;
using E_Commerce.Application.Common.Exceptions;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.UpdateBrand;

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, BrandDto>
{
    private readonly IBrandRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateBrandCommandHandler> _logger;

    public UpdateBrandCommandHandler(
        IBrandRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateBrandCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BrandDto> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (brand == null)
            throw new NotFoundException(nameof(Brand), request.Id);

        // Call individual behavior methods on the aggregate
        brand.UpdateDescription(new BrandDescription(request.Name, request.Description, request.LogoUrl));

        await _repository.UpdateAsync(brand, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Brand updated with ID {BrandId}", brand.Id);
        return _mapper.Map<BrandDto>(brand);
    }
}