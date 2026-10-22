using E_Commerce.Application.Shared.Scheduling.Attributes;
using E_Commerce.Application.Shared.Scheduling.Abstractions;

namespace E_Commerce.Infrastructure.Files.Jobs;

/// <summary>
/// Schedules the file storage cleanup job to run every 30 minutes.
/// </summary>
[RecurringJob("file-storage-cleanup", "*/30 * * * *")]
public sealed class FileStorageCleanupTrigger : IRecurringJobTrigger
{
    private readonly IJobScheduler _scheduler;

    public FileStorageCleanupTrigger(IJobScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public void Trigger()
    {
        _scheduler.Enqueue(new FileStorageCleanupJobPayload());
    }
}