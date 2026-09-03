using E_Commerce.Application.BoundedContexts.Catalog.Products.Validation;
using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Application.Shared.Models;
using E_Commerce.Domain.BoundedContexts.Core.Catalog.Repositories;
using E_Commerce.Domain.SharedKernel.Exceptions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using MediatR;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Commands.AddProductImage;

public sealed class AddProductImageCommandHandler : IRequestHandler<AddProductImageCommand, Result<Guid>>
{
    private readonly IProductRepository _productRepository;
    private readonly IFileService _fileService;
    private readonly ProductImageFileValidator _imageValidator;
    private readonly IUnitOfWork _unitOfWork;

    public AddProductImageCommandHandler(
        IProductRepository productRepository,
        IFileService fileService,
        ProductImageFileValidator imageValidator,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _fileService = fileService;
        _imageValidator = imageValidator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AddProductImageCommand command, CancellationToken ct)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
            if (product is null) return Result<Guid>.Failure("Product not found.");

            var validation = await _imageValidator.ValidateAsync(command.Image, ct);
            if (!validation.Succeeded) return Result<Guid>.Failure(validation.Errors);

            var fileId = await _fileService.UploadAsync(
                command.Image.Content,
                command.Image.FileName,
                command.Image.ContentType,
                ct);

            product.AddImage(fileId, command.Image.FileName);

            await _productRepository.UpdateAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<Guid>.Success(fileId);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}