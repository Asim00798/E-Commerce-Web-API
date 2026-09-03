using E_Commerce.Application.BoundedContexts.Catalog.Brands.Validation;
using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.Behaviors;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.CreateBrand;

public sealed class CreateBrandCommandHandler
    : IRequestHandler<CreateBrandCommand, Result<Guid>>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IFileService _fileService;
    private readonly BrandLogoFileValidator _logoValidator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBrandCommandHandler(
        IBrandRepository brandRepository,
        IFileService fileService,
        BrandLogoFileValidator logoValidator,
        IUnitOfWork unitOfWork)
    {
        _brandRepository = brandRepository;
        _fileService = fileService;
        _logoValidator = logoValidator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateBrandCommand command,
        CancellationToken ct)
    {
        try
        {
            var validation = await _logoValidator.ValidateAsync(command.Logo, ct);
            if (!validation.Succeeded)
                return Result<Guid>.Failure(validation.Errors);

            var logoFileId = await _fileService.UploadAsync(
                command.Logo.Content,
                command.Logo.FileName,
                command.Logo.ContentType,
                ct);

            var logo = new BrandLogo(logoFileId);

            var brand = Brand.Create(
                command.Name,
                command.DescriptionText,
                logo);

            await _brandRepository.AddAsync(brand, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<Guid>.Success(brand.Id);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}