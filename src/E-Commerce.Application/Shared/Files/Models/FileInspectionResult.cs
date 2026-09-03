namespace E_Commerce.Application.Shared.Files.Models;

/// <summary>
/// Generic technical result of inspecting a file's actual content.
/// Does not expose implementation details such as MIME mismatch.
/// </summary>
public sealed record FileInspectionResult(
    FileType DetectedType,
    FileValidationStatus ValidationStatus);