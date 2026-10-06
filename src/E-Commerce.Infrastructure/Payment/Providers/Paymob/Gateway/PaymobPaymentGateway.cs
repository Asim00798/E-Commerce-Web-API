#region Using Directives
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
#endregion

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
        // IntegrationId is validated at boot by PaymobOptionsValidator.
        var paymobRequest = new CreateIntentionRequest
        {
            AmountInMinorUnit = PaymobCurrencyConverter.ToMinorUnit(request.Amount),
            Currency = request.Amount.Currency,
            IntegrationId = int.Parse(_options.IntegrationId),
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

    /// <summary>
    /// Queries payment status.
    ///
    /// Prefers the transaction ID when present (post-capture payments).
    /// Falls back to the merchant order ID for payments that have not yet
    /// been captured — <c>AwaitingPayment</c> payments have an intention ID
    /// but no transaction ID, and Paymob has no intention-status endpoint.
    /// </summary>
    public async Task<PaymentStatusResult> GetPaymentStatusAsync(
        PaymentProviderReference reference,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(reference.TransactionId))
        {
            var response = await _apiClient.GetTransactionStatusAsync(
                reference.TransactionId, ct);

            return PaymobPaymentMapper.ToPaymentStatusResult(response);
        }

        if (!string.IsNullOrWhiteSpace(reference.MerchantOrderId))
        {
            var response = await _apiClient.GetTransactionStatusByMerchantOrderIdAsync(
                reference.MerchantOrderId, ct);

            return PaymobPaymentMapper.ToPaymentStatusResult(response);
        }

        throw new ArgumentException(
            "PaymentProviderReference must contain TransactionId or MerchantOrderId.",
            nameof(reference));
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

            // Pending refunds are treated as Unknown so reconciliation
            // resolves them via the parent transaction. Only definitive
            // success or failure short-circuits.
            var outcome = response.Success
                ? RefundOutcome.Succeeded
                : response.Pending
                    ? RefundOutcome.Unknown
                    : RefundOutcome.Failed;

            return new RefundResult
            {
                Outcome = outcome,
                ProviderTransactionId = response.TransactionId,
                ErrorMessage = response.Success
                    ? null
                    : response.Message ?? "Refund did not complete."
            };
        }
        catch (PaymobApiException ex)
        {
            _logger.LogError(
                ex,
                "Paymob refund API failure for transaction {TransactionId}",
                reference.TransactionId);

            var outcome = ex.StatusCode switch
            {
                HttpStatusCode.RequestTimeout => RefundOutcome.Unknown,
                HttpStatusCode.TooManyRequests => RefundOutcome.Unknown,
                HttpStatusCode.InternalServerError => RefundOutcome.Unknown,
                HttpStatusCode.BadGateway => RefundOutcome.Unknown,
                HttpStatusCode.ServiceUnavailable => RefundOutcome.Unknown,
                HttpStatusCode.GatewayTimeout => RefundOutcome.Unknown,
                _ when (int)(ex.StatusCode ?? 0) >= 500 => RefundOutcome.Unknown,
                _ => RefundOutcome.Failed
            };

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

    /// <summary>
    /// Reads refund outcome from the parent payment transaction.
    ///
    /// Paymob does not expose a dedicated refund-status endpoint. The
    /// supplied reference must carry the parent payment's transaction ID
    /// (the transaction that was refunded). The parent transaction's
    /// <c>is_refunded</c> field is the authoritative signal.
    /// </summary>
    public async Task<RefundStatusResult> GetRefundStatusAsync(
        PaymentProviderReference reference,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reference.TransactionId))
        {
            throw new ArgumentException(
                "Provider payment transaction ID is required for refund status inquiry.",
                nameof(reference));
        }

        var response = await _apiClient.GetTransactionStatusAsync(
            reference.TransactionId, ct);

        if (response.IsRefunded)
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
            ErrorMessage = "Refund did not complete."
        };
    }
}