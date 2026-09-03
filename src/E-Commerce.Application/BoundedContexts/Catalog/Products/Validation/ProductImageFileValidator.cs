using E_Commerce.Application.Shared.Files.Models;
using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Application.Shared.Models;

namespace E_Commerce.Application.BoundedContexts.Catalog.Products.Validation;

/// <summary>
/// Product-specific file validator.
/// Ensures the uploaded product image is a valid image.
/// </summary>
public sealed class ProductImageFileValidator
{
    private readonly IFileContentInspector _fileContentInspector;

    public ProductImageFileValidator(IFileContentInspector fileContentInspector)
    {
        _fileContentInspector = fileContentInspector;
    }

    public async Task<Result> ValidateAsync(FileUpload file, CancellationToken ct = default)
    {
        var inspection = await _fileContentInspector.InspectAsync(
            file.Content,
            FileType.Image,
            ct);

        if (inspection.ValidationStatus != FileValidationStatus.Valid)
            return Result.Failure("Product image must be a valid image.");

        return Result.Success();
    }
}