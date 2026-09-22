namespace E_Commerce.Api.DTOs.v1.Payments.Responses;

public sealed class PaymentInitiationResponse
{
    public string Provider { get; init; } = string.Empty;
    public string IntentionId { get; init; } = string.Empty;
    public string CheckoutUrl { get; init; } = string.Empty;
}