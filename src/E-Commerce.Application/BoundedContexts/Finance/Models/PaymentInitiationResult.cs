namespace E_Commerce.Application.BoundedContexts.Finance.Models;

public sealed record PaymentInitiationResult
{
    public string Provider { get; init; } = string.Empty;
    public string IntentionId { get; init; } = string.Empty;
    public string CheckoutUrl { get; init; } = string.Empty;
    public string? ClientSecret { get; init; }
}