using E_Commerce.Application.Modules.Scheduling.Abstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Files.Jobs
{
    public class CleanupTempFilesJob : IRecurringJob
    {
        public string CronExpression => "0 */6 * * *"; // every 6 hours
        private readonly ILogger<CleanupTempFilesJob> _logger;

        public CleanupTempFilesJob(ILogger<CleanupTempFilesJob> logger) => _logger = logger;

        public async Task ExecuteAsync(IJobContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Cleaning up temporary files");
            await Task.Delay(150, cancellationToken);
        }
    }
}


