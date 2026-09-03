using E_Commerce.Domain.BoundedContexts.Core.Shipping.Enums;
using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.AggregateRoots.Shipment.Entities;

/// <summary>
/// Represents a single delivery attempt for a shipment.
/// This entity is owned by the Shipment aggregate and must be manipulated
/// only through Shipment.RecordDeliveryAttempt(...).
/// </summary>
public sealed class DeliveryAttempt : BaseEntity
{
    public Guid ShipmentId { get; private set; }
    public int AttemptNumber { get; private set; }
    public DateTime AttemptedAtUtc { get; private set; }
    public DeliveryAttemptResult Result { get; private set; }
    public string? FailureReason { get; private set; }
    public string? Notes { get; private set; }

    private DeliveryAttempt()
    {
        // EF Core
    }

    internal DeliveryAttempt(
        Guid shipmentId,
        int attemptNumber,
        DeliveryAttemptResult result,
        string? failureReason = null,
        string? notes = null)
    {
        if (shipmentId == Guid.Empty)
            throw new ArgumentException("Shipment ID is required.", nameof(shipmentId));

        if (attemptNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(attemptNumber));

        ShipmentId = shipmentId;
        AttemptNumber = attemptNumber;
        AttemptedAtUtc = DateTime.UtcNow;
        Result = result;
        FailureReason = failureReason;
        Notes = notes;
    }
}