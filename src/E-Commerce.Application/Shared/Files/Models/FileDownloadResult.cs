using E_Commerce.Application.Shared.Files.Dtos;

namespace E_Commerce.Application.Shared.Files.Models;

public sealed record FileDownloadResult(
    FileDto Metadata,
    Stream Content);