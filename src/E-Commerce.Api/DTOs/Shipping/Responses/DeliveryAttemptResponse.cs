namespace E_Commerce.Api.DTOs.Shipping.Responses;

public sealed class DeliveryAttemptResponse
{
    public int AttemptNumber { get; set; }

    public DateTime AttemptedAtUtc { get; set; }

    public string Result { get; set; } = string.Empty;

    public string? FailureReason { get; set; }

    public string? Notes { get; set; }
}