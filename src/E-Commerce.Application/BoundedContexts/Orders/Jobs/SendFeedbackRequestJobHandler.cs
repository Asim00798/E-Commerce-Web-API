using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Orders.Jobs;

/// <summary>
/// Sends the "how was your purchase?" feedback email.
/// </summary>
public class SendFeedbackRequestJobHandler : IJobHandler<SendFeedbackRequestJob>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendFeedbackRequestJobHandler> _logger;

    public SendFeedbackRequestJobHandler(
        IEmailService emailService,
        ILogger<SendFeedbackRequestJobHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(SendFeedbackRequestJob job, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending feedback request for order {OrderId}", job.OrderId);
        await _emailService.SendFeedbackRequestAsync(job.OrderId, job.CustomerEmail);
    }
}