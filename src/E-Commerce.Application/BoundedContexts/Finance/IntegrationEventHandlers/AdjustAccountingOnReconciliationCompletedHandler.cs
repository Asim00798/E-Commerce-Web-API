using E_Commerce.Application.BoundedContexts.Finance.Abstractions;
using E_Commerce.Application.BoundedContexts.Inventory.IntegrationEvents;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Finance.IntegrationEventHandlers;

public class AdjustAccountingOnReconciliationCompletedHandler
    : IIntegrationEventHandler<InventoryReconciliationCompletedIntegrationEvent>
{
    private readonly IAccountingService _accountingService;
    private readonly ILogger<AdjustAccountingOnReconciliationCompletedHandler> _logger;

    public AdjustAccountingOnReconciliationCompletedHandler(
        IAccountingService accountingService,
        ILogger<AdjustAccountingOnReconciliationCompletedHandler> logger)
    {
        _accountingService = accountingService;
        _logger = logger;
    }

    public async Task HandleAsync(
        InventoryReconciliationCompletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adjusting accounting for reconciliation {ReconciliationId} ({DiscrepancyCount} discrepancies)",
            integrationEvent.ReconciliationId,
            integrationEvent.DiscrepancyCount);

        await _accountingService.CreateAdjustmentEntriesAsync(
            integrationEvent.ReconciliationId,
            integrationEvent.DiscrepancyCount,
            cancellationToken);
    }
}