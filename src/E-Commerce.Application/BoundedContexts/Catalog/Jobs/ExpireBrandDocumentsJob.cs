using E_Commerce.Application.Modules.Scheduling.Abstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Catalog.Jobs
{
    public class ExpireBrandDocumentsJob : IRecurringJob
    {
        public string CronExpression => "0 0 * * *"; // daily at midnight
        private readonly ILogger<ExpireBrandDocumentsJob> _logger;

        public ExpireBrandDocumentsJob(ILogger<ExpireBrandDocumentsJob> logger) => _logger = logger;

        public async Task ExecuteAsync(IJobContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Expiring old documents");
            await Task.Delay(200, cancellationToken);
        }
    }
}
