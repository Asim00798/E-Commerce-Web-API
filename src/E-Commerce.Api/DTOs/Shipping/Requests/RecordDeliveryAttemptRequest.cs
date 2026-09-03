using E_Commerce.Domain.BoundedContexts.Core.Shipping.Enums;

namespace E_Commerce.Api.DTOs.Shipping.Requests;

public sealed class RecordDeliveryAttemptRequest
{
    public DeliveryAttemptResult Result { get; set; }

    public string? FailureReason { get; set; }

    public string? Notes { get; set; }
}