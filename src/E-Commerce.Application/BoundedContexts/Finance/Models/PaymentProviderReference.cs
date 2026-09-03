namespace E_Commerce.Application.BoundedContexts.Finance.Models;

public sealed record PaymentProviderReference
{
    public string Provider { get; init; } = string.Empty;
    public string? IntentionId { get; init; }
    public string? TransactionId { get; init; }
}