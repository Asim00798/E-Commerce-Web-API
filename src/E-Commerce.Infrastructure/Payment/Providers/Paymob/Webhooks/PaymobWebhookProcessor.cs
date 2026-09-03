using System.Text.Json;
using E_Commerce.Application.BoundedContexts.Finance.Abstractions;
using E_Commerce.Application.BoundedContexts.Finance.Commands.HandlePaymentWebhook;
using E_Commerce.Application.BoundedContexts.Finance.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Webhooks;

public sealed class PaymobWebhookProcessor : IPaymentWebhookProcessor
{
    private readonly ISender _sender;
    private readonly PaymobHmacVerifier _hmacVerifier;
    private readonly ILogger<PaymobWebhookProcessor> _logger;

    public PaymobWebhookProcessor(
        ISender sender,
        PaymobHmacVerifier hmacVerifier,
        ILogger<PaymobWebhookProcessor> logger)
    {
        _sender = sender;
        _hmacVerifier = hmacVerifier;
        _logger = logger;
    }

    public async Task<PaymentWebhookCommandResult> ProcessAsync(
        string provider,
        string payload,
        string? signature,
        CancellationToken ct = default)
    {
        if (!string.Equals(provider, "Paymob", StringComparison.OrdinalIgnoreCase))
        {
            return PaymentWebhookCommandResult.Failure(
                "Unsupported webhook provider.",
                PaymentWebhookErrorType.Validation);
        }

        if (string.IsNullOrWhiteSpace(signature))
        {
            return PaymentWebhookCommandResult.Failure(
                "Missing HMAC signature.",
                PaymentWebhookErrorType.Unauthorized);
        }

        TransactionCallback? callback;

        try
        {
            callback = JsonSerializer.Deserialize<TransactionCallback>(
                payload,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid Paymob webhook payload.");

            return PaymentWebhookCommandResult.Failure(
                "Invalid webhook payload.",
                PaymentWebhookErrorType.Validation);
        }

        if (callback is null)
        {
            return PaymentWebhookCommandResult.Failure(
                "Invalid webhook payload.",
                PaymentWebhookErrorType.Validation);
        }

        // HMAC verification before semantic validation.
        if (!_hmacVerifier.Verify(callback, signature))
        {
            return PaymentWebhookCommandResult.Failure(
                "Invalid HMAC signature.",
                PaymentWebhookErrorType.Unauthorized);
        }

        if (string.IsNullOrWhiteSpace(callback.TransactionId) ||
            string.IsNullOrWhiteSpace(callback.IntentionId))
        {
            return PaymentWebhookCommandResult.Failure(
                "Webhook payload is missing required provider identifiers.",
                PaymentWebhookErrorType.Validation);
        }

        var command = new HandlePaymentWebhookCommand(
            Provider: "Paymob",
            ProviderTransactionId: callback.TransactionId,
            ProviderIntentionId: callback.IntentionId,
            Success: callback.Success,
            Message: callback.ErrorOccurred ? "Payment failed" : null);

        return await _sender.Send(command, ct);
    }
}