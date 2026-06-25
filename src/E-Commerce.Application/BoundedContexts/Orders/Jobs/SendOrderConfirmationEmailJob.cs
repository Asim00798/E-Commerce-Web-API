using E_Commerce.Application.Modules.Scheduling.Abstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Orders.Jobs
{
    public class SendOrderConfirmationEmailJob : IJob
    {
        private readonly ILogger<SendOrderConfirmationEmailJob> _logger;

        public SendOrderConfirmationEmailJob(ILogger<SendOrderConfirmationEmailJob> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(IJobContext context, CancellationToken cancellationToken = default)
        {
            var orderId = context.GetData<Guid>("orderId");
            _logger.LogInformation("Sending order confirmation email for order {OrderId}", orderId);
            // Simulate email sending
            await Task.Delay(100, cancellationToken);
        }
    }
}
