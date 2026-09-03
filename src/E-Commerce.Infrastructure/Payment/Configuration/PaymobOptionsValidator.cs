using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Payment.Configuration;

public sealed class PaymobOptionsValidator : IValidateOptions<PaymobOptions>
{
    public ValidateOptionsResult Validate(string? name, PaymobOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            errors.Add("Payment:Paymob:ApiKey is required.");

        if (string.IsNullOrWhiteSpace(options.SecretKey))
            errors.Add("Payment:Paymob:SecretKey is required.");

        if (string.IsNullOrWhiteSpace(options.IntegrationId))
            errors.Add("Payment:Paymob:IntegrationId is required.");

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            errors.Add("Payment:Paymob:BaseUrl is required.");
        else if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri) ||
                 (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            errors.Add("Payment:Paymob:BaseUrl must be a valid absolute HTTP/HTTPS URL.");

        if (string.IsNullOrWhiteSpace(options.WebhookSecret))
            errors.Add("Payment:Paymob:WebhookSecret is required.");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}