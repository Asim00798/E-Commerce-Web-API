using E_Commerce.Application.BoundedContexts.Catalog.Brands.Validation;
using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.AggregateRoots.Brand.ValueObjects;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Commands.UpdateBrand;

public sealed class UpdateBrandCommandHandler
    : IRequestHandler<UpdateBrandCommand, Result>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IFileService _fileService;
    private readonly BrandLogoFileValidator _logoValidator;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBrandCommandHandler(
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

    public async Task<Result> Handle(
        UpdateBrandCommand command,
        CancellationToken ct)
    {
        try
        {
            var brand = await _brandRepository.GetByIdAsync(command.BrandId, ct);
            if (brand is null)
                return Result.Failure("Brand not found.");

            if (!string.IsNullOrWhiteSpace(command.Name))
                brand.UpdateName(command.Name);

            if (!string.IsNullOrWhiteSpace(command.DescriptionText))
                brand.UpdateDescription(command.DescriptionText);

            if (command.NewLogo is not null)
            {
                var validation = await _logoValidator.ValidateAsync(command.NewLogo, ct);
                if (!validation.Succeeded)
                    return Result.Failure(validation.Errors);

                var newFileId = await _fileService.UploadAsync(
                    command.NewLogo.Content,
                    command.NewLogo.FileName,
                    command.NewLogo.ContentType,
                    ct);

                brand.UpdateLogo(new BrandLogo(newFileId));
            }

            await _brandRepository.UpdateAsync(brand, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}