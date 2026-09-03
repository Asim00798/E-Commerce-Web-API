namespace E_Commerce.Application.Shared.Files.Models;

/// <summary>
/// Represents an application-level file upload input.
/// The API layer translates IFormFile into this model.
/// </summary>
public sealed record FileUpload(
    Stream Content,
    string FileName,
    string ContentType);
