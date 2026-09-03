namespace E_Commerce.Api.DTOs.Shipping.Responses;

public sealed class ShipmentResponse
{
    public Guid ShipmentId { get; set; }

    public Guid OrderId { get; set; }

    public Guid CustomerId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string TrackingNumber { get; set; } = string.Empty;

    public Guid? AssignedDriverId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string LocationMapUrl { get; set; } = string.Empty;

    public IReadOnlyList<DeliveryAttemptResponse> DeliveryAttempts { get; set; } =
        Array.Empty<DeliveryAttemptResponse>();
}