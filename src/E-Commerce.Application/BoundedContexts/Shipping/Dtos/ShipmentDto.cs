namespace E_Commerce.Application.BoundedContexts.Shipping.Dtos;

public sealed class ShipmentDto
{
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string TrackingNumber { get; init; } = string.Empty;
    public Guid? AssignedDriverId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string LocationMapUrl { get; init; } = string.Empty;
    public IReadOnlyList<DeliveryAttemptDto> DeliveryAttempts { get; init; } = Array.Empty<DeliveryAttemptDto>();
}