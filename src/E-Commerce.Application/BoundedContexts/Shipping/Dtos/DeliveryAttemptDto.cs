namespace E_Commerce.Application.BoundedContexts.Shipping.Dtos;

public sealed class DeliveryAttemptDto
{
    public int AttemptNumber { get; init; }
    public DateTime AttemptedAtUtc { get; init; }
    public string Result { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
    public string? Notes { get; init; }
}