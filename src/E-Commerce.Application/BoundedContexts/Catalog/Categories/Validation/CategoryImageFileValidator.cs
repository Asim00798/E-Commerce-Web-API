using E_Commerce.Application.Shared.Files.Models;
using E_Commerce.Application.Shared.Files.Services;
using E_Commerce.Application.Shared.Models;

namespace E_Commerce.Application.BoundedContexts.Catalog.Categories.Validation;

/// <summary>
/// Category-specific file validator.
/// Ensures the uploaded category image is a valid image.
/// </summary>
public sealed class CategoryImageFileValidator
{
    private readonly IFileContentInspector _fileContentInspector;

    public CategoryImageFileValidator(IFileContentInspector fileContentInspector)
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
            return Result.Failure("Category image must be a valid image.");

        return Result.Success();
    }
}