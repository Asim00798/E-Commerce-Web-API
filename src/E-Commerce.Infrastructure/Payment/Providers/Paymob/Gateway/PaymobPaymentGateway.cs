using E_Commerce.Application.BoundedContexts.Finance.Abstractions;
using E_Commerce.Application.BoundedContexts.Finance.Models;
using E_Commerce.Domain.SharedKernel.ValueObjects;
using E_Commerce.Infrastructure.Payment.Configuration;
using E_Commerce.Infrastructure.Payment.Providers.Paymob.Api.Client;
using E_Commerce.Infrastructure.Payment.Providers.Paymob.Api.Models;
using E_Commerce.Infrastructure.Payment.Providers.Paymob.Conversion;
using E_Commerce.Infrastructure.Payment.Providers.Paymob.Exceptions;
using E_Commerce.Infrastructure.Payment.Providers.Paymob.Mapping;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Gateway;

public sealed class PaymobPaymentGateway : IPaymentGateway
{
    private readonly PaymobApiClient _apiClient;
    private readonly PaymobOptions _options;
    private readonly ILogger<PaymobPaymentGateway> _logger;

    public PaymobPaymentGateway(
        PaymobApiClient apiClient,
        IOptions<PaymobOptions> options,
        ILogger<PaymobPaymentGateway> logger)
    {
        _apiClient = apiClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PaymentInitiationResult> InitiatePaymentAsync(
        PaymentInitiationRequest request,
        CancellationToken ct = default)
    {
        if (!int.TryParse(_options.IntegrationId, out var integrationId))
        {
            throw new InvalidOperationException("Paymob integration ID must be a valid integer.");
        }

        var paymobRequest = new CreateIntentionRequest
        {
            AmountInMinorUnit = PaymobCurrencyConverter.ToMinorUnit(request.Amount),
            Currency = request.Amount.Currency,
            IntegrationId = integrationId,
            MerchantOrderId = request.OrderId.ToString(),
            ReturnUrl = request.ReturnUrl,
            CancelUrl = request.CancelUrl,
            IdempotencyKey = request.IdempotencyKey ?? request.OrderId.ToString()
        };

        var response = await _apiClient.CreateIntentionAsync(paymobRequest, ct);

        return new PaymentInitiationResult
        {
            Provider = "Paymob",
            IntentionId = response.IntentionId,
            CheckoutUrl = response.CheckoutUrl ?? string.Empty,
            ClientSecret = response.ClientSecret
        };
    }

    public async Task<PaymentStatusResult> GetPaymentStatusAsync(
        PaymentProviderReference reference,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reference.TransactionId))
        {
            throw new ArgumentException(
                "Provider transaction ID is required for status inquiry.",
                nameof(reference));
        }

        var response = await _apiClient.GetTransactionStatusAsync(reference.TransactionId, ct);

        return PaymobPaymentMapper.ToPaymentStatusResult(response);
    }

    public async Task<RefundResult> RefundAsync(
        PaymentProviderReference reference,
        Money amount,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reference.TransactionId))
        {
            throw new ArgumentException(
                "Provider transaction ID is required for refund.",
                nameof(reference));
        }

        try
        {
            var amountMinor = PaymobCurrencyConverter.ToMinorUnit(amount);

            var response = await _apiClient.RefundAsync(
                reference.TransactionId,
                amountMinor,
                ct);

            return new RefundResult
            {
                Outcome = response.Success
                    ? RefundOutcome.Succeeded
                    : RefundOutcome.Failed,
                ProviderTransactionId = response.TransactionId,
                ErrorMessage = response.Success ? null : response.Message ?? "Refund failed."
            };
        }
        catch (PaymobApiException ex)
        {
            _logger.LogError(
                ex,
                "Paymob refund API failure for transaction {TransactionId}",
                reference.TransactionId);

            var outcome = ex.StatusCode is HttpStatusCode statusCode && (int)statusCode >= 500
                ? RefundOutcome.Unknown
                : RefundOutcome.Failed;

            return new RefundResult
            {
                Outcome = outcome,
                ProviderTransactionId = null,
                ErrorMessage = ex.Message
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Paymob refund transport failure for transaction {TransactionId}. Outcome unknown.",
                reference.TransactionId);

            return new RefundResult
            {
                Outcome = RefundOutcome.Unknown,
                ProviderTransactionId = null,
                ErrorMessage = "Payment provider could not be reached. Outcome unknown."
            };
        }
    }

    public async Task<RefundStatusResult> GetRefundStatusAsync(
    PaymentProviderReference reference,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reference.TransactionId))
        {
            throw new ArgumentException(
                "Provider transaction ID is required for refund status inquiry.",
                nameof(reference));
        }

        var response = await _apiClient.GetRefundStatusAsync(
            reference.TransactionId,
            ct);

        if (response.Success)
        {
            return new RefundStatusResult
            {
                Outcome = RefundOutcome.Succeeded,
                ProviderTransactionId = response.TransactionId,
                ErrorMessage = null
            };
        }

        if (response.Pending)
        {
            return new RefundStatusResult
            {
                Outcome = RefundOutcome.Unknown,
                ProviderTransactionId = response.TransactionId,
                ErrorMessage = "Refund is pending."
            };
        }

        return new RefundStatusResult
        {
            Outcome = RefundOutcome.Failed,
            ProviderTransactionId = response.TransactionId,
            ErrorMessage = response.Message ?? "Refund failed."
        };
    }
}