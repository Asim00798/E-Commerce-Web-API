namespace E_Commerce.Application.BoundedContexts.Finance.Models;

public sealed record RefundStatusResult
{
    public RefundOutcome Outcome { get; init; }
    public string? ProviderTransactionId { get; init; }
    public string? ErrorMessage { get; init; }
}
public enum RefundOutcome
{
    Succeeded = 1,
    Failed = 2,
    Unknown = 3
}