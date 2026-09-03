using E_Commerce.Application.BoundedContexts.Orders.Abstractions;
using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.BoundedContexts.Orders.Models;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace E_Commerce.Application.BoundedContexts.Orders.Jobs.ExpirePendingOrders;

public sealed class ExpirePendingOrdersJobHandler : IJobHandler<ExpirePendingOrdersJob>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly ILogger<ExpirePendingOrdersJobHandler> _logger;
    private readonly IPendingOrderCleanupService _cleanupService;
    private readonly OrderingOptions _options;

    public ExpirePendingOrdersJobHandler(
        IUnitOfWork unitOfWork,
        IOutboxMessageWriter outboxWriter,
        ILogger<ExpirePendingOrdersJobHandler> logger,
        IPendingOrderCleanupService cleanupService,
        IOptions<OrderingOptions> options)
    {
        _unitOfWork = unitOfWork;
        _outboxWriter = outboxWriter;
        _logger = logger;
        _cleanupService = cleanupService;
        _options = options.Value;
    }

    public async Task HandleAsync(ExpirePendingOrdersJob job, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting expiration of pending orders.");

        var expiredOrderIds = await _cleanupService.ExpirePendingOrdersAsync(
            TimeSpan.FromHours(_options.PendingOrderExpirationHours),
            cancellationToken);

        if (expiredOrderIds.Count == 0)
        {
            _logger.LogInformation("No expired orders found.");
            return;
        }

        // Publish a single business fact for all expired orders
        var integrationEvent = new OrdersExpiredIntegrationEvent(
            expiredOrderIds: expiredOrderIds,
            DateTime.UtcNow);

        await _outboxWriter.WriteAsync(integrationEvent, cancellationToken);

        // Atomic save: order state changes + outbox message
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Expired {Count} pending orders.", expiredOrderIds.Count);
    }
}