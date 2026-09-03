using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Infrastructure.Files.Abstractions;

namespace E_Commerce.Infrastructure.Files.Jobs;

public sealed class FileStorageCleanupJobHandler
    : IJobHandler<FileStorageCleanupJobPayload>
{
    private readonly IFileStorageCleanupService _cleanupService;

    public FileStorageCleanupJobHandler(IFileStorageCleanupService cleanupService)
    {
        _cleanupService = cleanupService;
    }

    public async Task HandleAsync(
        FileStorageCleanupJobPayload job,
        CancellationToken ct)
    {
        await _cleanupService.ExecuteAsync(ct);
    }
}