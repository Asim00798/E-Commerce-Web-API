namespace E_Commerce.Api.DTOs.v1.Shipping.Responses;

public sealed class ShipmentResponse
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

    public IReadOnlyList<DeliveryAttemptResponse> DeliveryAttempts { get; init; } =
        Array.Empty<DeliveryAttemptResponse>();
}