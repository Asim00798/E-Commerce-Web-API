namespace E_Commerce.Infrastructure.Payment.Configuration;

public sealed class PaymobOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string IntegrationId { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://accept.paymob.com/api";

    public string WebhookSecret { get; set; } = string.Empty;
}