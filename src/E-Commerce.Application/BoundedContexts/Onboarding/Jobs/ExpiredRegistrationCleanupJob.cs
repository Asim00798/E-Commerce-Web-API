using E_Commerce.Application.Modules.Scheduling.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Jobs;

/// <summary>
/// Marker payload for the recurring job that deletes expired registration aggregates.
/// The handler contains all the logic; this class simply signals the job.
/// </summary>
public sealed class ExpiredRegistrationCleanupJob : IJob
{}