namespace E_Commerce.Application.Shared.Files.Dtos;

public sealed record FileDto(
    Guid Id,
    string FileName,
    string ContentType,
    long Size,
    DateTime CreatedAtUtc);