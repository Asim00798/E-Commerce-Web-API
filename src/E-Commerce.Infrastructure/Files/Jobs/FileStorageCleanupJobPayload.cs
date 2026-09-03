using E_Commerce.Application.Modules.Scheduling.Abstractions;

namespace E_Commerce.Infrastructure.Files.Jobs;

/// <summary>
/// Payload for the file storage cleanup job.
/// The job performs:
/// - processing pending deletions,
/// - orphaned physical file cleanup.
/// </summary>
public sealed class FileStorageCleanupJobPayload : IJob
{
    // No parameters required for this job.
}