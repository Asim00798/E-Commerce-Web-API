namespace E_Commerce.Application.BoundedContexts.Finance.Models;

/// <summary>
/// Identifies a payment to the provider for status or refund operations.
///
/// At least one of <see cref="TransactionId"/> or <see cref="MerchantOrderId"/>
/// must be present for status queries. <see cref="IntentionId"/> is retained
/// for audit but is not queryable — Paymob has no intention-status endpoint.
/// </summary>
public sealed record PaymentProviderReference
{
    public string Provider { get; init; } = string.Empty;

    public string? IntentionId { get; init; }

    public string? TransactionId { get; init; }

    public string? MerchantOrderId { get; init; }
}