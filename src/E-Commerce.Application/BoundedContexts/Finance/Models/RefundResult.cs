namespace E_Commerce.Application.BoundedContexts.Finance.Models;

public sealed record RefundResult
{
    public RefundOutcome Outcome { get; init; } = RefundOutcome.Failed;

    public bool Succeeded => Outcome == RefundOutcome.Succeeded;

    public string? ProviderTransactionId { get; init; }

    public string? ErrorMessage { get; init; }
}