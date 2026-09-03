using E_Commerce.Application.BoundedContexts.Orders.Models;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Abstractions;
using E_Commerce.Application.Shared.Communication.Notifications.Channels;
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;
using E_Commerce.Domain.BoundedContexts.UserManagement.Registration.Repositories;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Orders.Jobs.SendFeedbackRequest;

public sealed class SendFeedbackRequestJobHandler : IJobHandler<SendFeedbackRequestJob>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IEmailChannel _emailChannel;
    private readonly ILogger<SendFeedbackRequestJobHandler> _logger;

    public SendFeedbackRequestJobHandler(
        IOrderRepository orderRepository,
        IPersonRepository personRepository,
        IEmailChannel emailChannel,
        ILogger<SendFeedbackRequestJobHandler> logger)
    {
        _orderRepository = orderRepository;
        _personRepository = personRepository;
        _emailChannel = emailChannel;
        _logger = logger;
    }

    public async Task HandleAsync(SendFeedbackRequestJob job, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(job.OrderId, cancellationToken);
        if (order is null)
        {
            _logger.LogWarning("Order {OrderId} not found for feedback request.", job.OrderId);
            return;
        }

        var person = await _personRepository.GetByIdentityUserIdAsync(order.CustomerId, cancellationToken);
        if (person is null)
        {
            _logger.LogWarning("Person record not found for customer {CustomerId}.", order.CustomerId);
            return;
        }

        var model = new FeedbackRequestEmail
        {
            RecipientEmail = person.Email.Value,
            CustomerName = person.Name.ToString(),
            OrderId = order.Id
        };

        await _emailChannel.SendAsync(new NotificationRequest<FeedbackRequestEmail>
        {
            UserId = order.CustomerId,
            Model = model
        }, cancellationToken);

        _logger.LogInformation("Feedback request email queued for order {OrderId}.", order.Id);
    }
}