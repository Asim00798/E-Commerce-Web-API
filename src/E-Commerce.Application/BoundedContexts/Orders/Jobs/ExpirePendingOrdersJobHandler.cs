using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;   // IOutboxMessageWriter
using E_Commerce.Domain.BoundedContexts.Core.Ordering.Repositories;        // IOrderRepository
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;               // IUnitOfWork
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Orders.Jobs;

public class ExpirePendingOrdersJobHandler : IJobHandler<ExpirePendingOrdersJob>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly ILogger<ExpirePendingOrdersJobHandler> _logger;

    public ExpirePendingOrdersJobHandler(
        IUnitOfWork unitOfWork,
        IOrderRepository orderRepository,
        IOutboxMessageWriter outboxWriter,
        ILogger<ExpirePendingOrdersJobHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    public async Task HandleAsync(ExpirePendingOrdersJob job, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting expiration of pending orders.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        // Load orders that have exceeded the pending time threshold
        var expiredOrders = await _orderRepository.GetPendingOrdersOlderThanAsync(TimeSpan.FromMinutes(30));

        if (expiredOrders.Count == 0)
        {
            _logger.LogInformation("No expired orders found.");
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return;
        }

        // Mark each order as expired (aggregate operation)
        foreach (var order in expiredOrders)
        {
            order.Expire();
        }

        // Publish the business fact – a single integration event with all affected order IDs
        var integrationEvent = new OrdersExpiredIntegrationEvent(
            expiredOrderIds: expiredOrders.Select(o => o.Id).ToList(),
            expiredCount: expiredOrders.Count,
            expiredAt: DateTime.UtcNow);

        await _outboxWriter.WriteAsync(integrationEvent, cancellationToken);

        // Atomically save the order state changes and the outbox message, then commit
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        _logger.LogInformation("Expired {Count} pending orders.", expiredOrders.Count);
    }
}