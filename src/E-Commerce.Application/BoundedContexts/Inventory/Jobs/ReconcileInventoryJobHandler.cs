using E_Commerce.Application.BoundedContexts.Inventory.IntegrationEvents;
using E_Commerce.Application.Modules.Scheduling.Abstractions;
using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;   // IOutboxMessageWriter
using E_Commerce.Domain.BoundedContexts.Core.Inventory.Repositories;        // IInventoryRepository
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;               // IUnitOfWork
using Microsoft.Extensions.Logging;

namespace E_Commerce.Application.BoundedContexts.Inventory.Jobs;

/// <summary>
/// Handles the monthly inventory reconciliation job.
/// Merges the business logic, transaction management, and event publishing
/// into a single handler executed through the Controlled Execution Gateway.
/// </summary>
public class ReconcileInventoryJobHandler : IJobHandler<ReconcileInventoryJob>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IOutboxMessageWriter _outboxWriter;
    private readonly ILogger<ReconcileInventoryJobHandler> _logger;

    public ReconcileInventoryJobHandler(
        IUnitOfWork unitOfWork,
        IInventoryRepository inventoryRepository,
        IOutboxMessageWriter outboxWriter,
        ILogger<ReconcileInventoryJobHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _inventoryRepository = inventoryRepository;
        _outboxWriter = outboxWriter;
        _logger = logger;
    }

    // <summary>
    // The job parameter is unused because ReconcileInventoryJob is a marker job with no data—all
    // the handler needs is injected via DI.
    // That's completely normal and intentional for jobs that require no external parameters.
    // </summary>
    public async Task HandleAsync(ReconcileInventoryJob job, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting monthly inventory reconciliation.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        // Domain‑driven reconciliation – loads inventory aggregates, applies adjustments
        var discrepancies = await _inventoryRepository.FindAndApplyAdjustmentsAsync();

        // Publish the business fact through the enriched Outbox writer
        var integrationEvent = new InventoryReconciliationCompletedIntegrationEvent(
            reconciliationId: Guid.NewGuid(),
            discrepancyCount: discrepancies.Count,
            reconciledAt: DateTime.UtcNow);

        await _outboxWriter.WriteAsync(integrationEvent, cancellationToken);

        // Atomically save adjustments and outbox message, then commit
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        _logger.LogInformation(
            "Monthly inventory reconciliation completed with {Count} discrepancies.",
            discrepancies.Count);
    }
}