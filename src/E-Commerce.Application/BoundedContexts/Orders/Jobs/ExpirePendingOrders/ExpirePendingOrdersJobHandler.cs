using E_Commerce.Application.BoundedContexts.Orders.Abstractions;
using E_Commerce.Application.BoundedContexts.Orders.Configuration;
using E_Commerce.Application.BoundedContexts.Orders.IntegrationEvents;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace E_Commerce.Application.BoundedContexts.Orders.Jobs.ExpirePendingOrders;

/// <summary>
/// Detects pending orders that have exceeded the configured expiration
/// threshold and publishes a single OrdersExpiredIntegrationEvent for the batch.
///
/// This handler does NOT mutate order state or restore stock. Cancellation
/// and stock restoration are performed by dedicated integration event handlers
/// reacting to the published event.
/// </summary>
public sealed class ExpirePendingOrdersJobHandler : IJobHandler<ExpirePendingOrdersJob>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly IPendingOrderCleanupService _cleanupService;
    private readonly ILogger<ExpirePendingOrdersJobHandler> _logger;
    private readonly OrderingOptions _options;

    public ExpirePendingOrdersJobHandler(
        IUnitOfWork unitOfWork,
        IOutboxMessageWriter outboxWriter,
        IPendingOrderCleanupService cleanupService,
        ILogger<ExpirePendingOrdersJobHandler> logger,
        IOptions<OrderingOptions> options)
    {
        _unitOfWork = unitOfWork;
        _outboxWriter = outboxWriter;
        _cleanupService = cleanupService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task HandleAsync(
        ExpirePendingOrdersJob job,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scanning for expired pending orders.");

        var expiredOrderIds = await _cleanupService.GetExpiredPendingOrderIdsAsync(
            TimeSpan.FromHours(_options.PendingOrderExpirationHours),
            cancellationToken);

        if (expiredOrderIds.Count == 0)
        {
            _logger.LogInformation("No expired pending orders found.");
            return;
        }

        var integrationEvent = new OrdersExpiredIntegrationEvent(
            expiredOrderIds: expiredOrderIds,
            expiredAt: DateTime.UtcNow);

        await _outboxWriter.WriteAsync(integrationEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Published expiration event for {Count} pending orders.",
            expiredOrderIds.Count);
    }
}