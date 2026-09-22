namespace E_Commerce.Api.DTOs.v1.Shipping.Responses;

public sealed class DeliveryAttemptResponse
{
    public int AttemptNumber { get; init; }

    public DateTime AttemptedAtUtc { get; init; }

    public string Result { get; init; } = string.Empty;

    public string? FailureReason { get; init; }

    public string? Notes { get; init; }
}