namespace E_Commerce.Domain.BoundedContexts.Core.Shipping.Enums;

public enum ShipmentStatus
{
    Created = 1,
    Assigned = 2,
    ReadyForPickup = 3,
    PickedUp = 4,
    OutForDelivery = 5,
    ReturnToSender = 6,
    Delivered = 7,
    Returned = 8,
    Cancelled = 9
}