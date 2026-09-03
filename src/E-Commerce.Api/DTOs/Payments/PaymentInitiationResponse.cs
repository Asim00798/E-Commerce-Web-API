namespace E_Commerce.Api.DTOs.Payments.Responses;

public sealed class PaymentInitiationResponse
{
    public string Provider { get; set; } = string.Empty;
    public string IntentionId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
}