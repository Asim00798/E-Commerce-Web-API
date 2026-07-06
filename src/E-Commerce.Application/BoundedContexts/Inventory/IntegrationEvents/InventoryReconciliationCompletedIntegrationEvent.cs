using E_Commerce.Application.Shared.Communication.Messaging.Abstractions;

namespace E_Commerce.Application.BoundedContexts.Inventory.IntegrationEvents;

public class InventoryReconciliationCompletedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public Guid ReconciliationId { get; }
    public int DiscrepancyCount { get; }
    public DateTime ReconciledAt { get; }

    public InventoryReconciliationCompletedIntegrationEvent(
        Guid reconciliationId,
        int discrepancyCount,
        DateTime reconciledAt)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        ReconciliationId = reconciliationId;
        DiscrepancyCount = discrepancyCount;
        ReconciledAt = reconciledAt;
    }
}