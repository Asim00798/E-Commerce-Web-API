using E_Commerce.Application.Shared.Files.Models;

namespace E_Commerce.Application.Shared.Files.Services;

/// <summary>
/// Generic technical capability for inspecting uploaded file content.
/// The caller provides the expected file type to enable technical validation.
/// </summary>
public interface IFileContentInspector
{
    Task<FileInspectionResult> InspectAsync(
        Stream content,
        FileType expectedType,
        CancellationToken ct = default);
}