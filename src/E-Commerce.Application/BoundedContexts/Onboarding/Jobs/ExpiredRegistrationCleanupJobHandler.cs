using E_Commerce.Application.BoundedContexts.Onboarding.Abstractions;
using E_Commerce.Application.BoundedContexts.Onboarding.Jobs;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Domain.SharedKernel.Services;   // IClock lives here
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Onboarding.Jobs;

/// <summary>
/// Handles the <see cref="ExpiredRegistrationCleanupJob"/> by delegating
/// to the <see cref="IRegistrationCleanupService"/>.
/// This is a Housekeeping Job – no integration events are published.
/// </summary>
public sealed class ExpiredRegistrationCleanupJobHandler : IJobHandler<ExpiredRegistrationCleanupJob>
{
    private readonly IRegistrationCleanupService _cleanupService;
    private readonly IClock _clock;
    private readonly ILogger<ExpiredRegistrationCleanupJobHandler> _logger;

    public ExpiredRegistrationCleanupJobHandler(
        IRegistrationCleanupService cleanupService,
        IClock clock,
        ILogger<ExpiredRegistrationCleanupJobHandler> logger)
    {
        _cleanupService = cleanupService;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task HandleAsync(ExpiredRegistrationCleanupJob job, CancellationToken cancellationToken)
    {
        var deleted = await _cleanupService.DeleteExpiredAsync(_clock.UtcNow, cancellationToken);

        if (deleted > 0)
        {
            _logger.LogInformation("Expired registration cleanup removed {Count} records.", deleted);
        }
    }
}