using AutoMapper;
using E_Commerce.Application.BoundedContexts.Catalog.Brands.DTOs;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.CreateBrand;

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, BrandDto>
{
    private readonly IBrandRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBrandCommandHandler> _logger;

    public CreateBrandCommandHandler(
        IBrandRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateBrandCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BrandDto> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var description = new BrandDescription(request.Name, request.Description, request.LogoUrl);
        var brand = Brand.Create(description);

        await _repository.AddAsync(brand, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Brand created with ID {BrandId}", brand.Id);
        return _mapper.Map<BrandDto>(brand);
    }
}