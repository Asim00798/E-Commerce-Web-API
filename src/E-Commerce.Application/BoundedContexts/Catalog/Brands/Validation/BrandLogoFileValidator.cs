using E_Commerce.Application.Shared.Files.Models;
using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Application.Shared.Models;

namespace E_Commerce.Application.BoundedContexts.Catalog.Brands.Validation;

/// <summary>
/// Brand-specific file validator.
/// The Brand use case requires an Image and requires the inspector to confirm
/// the file is technically a valid image.
/// </summary>
public sealed class BrandLogoFileValidator
{
    private readonly IFileContentInspector _fileContentInspector;

    public BrandLogoFileValidator(IFileContentInspector fileContentInspector)
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
            return Result.Failure("Brand logo must be a valid image.");

        return Result.Success();
    }
}