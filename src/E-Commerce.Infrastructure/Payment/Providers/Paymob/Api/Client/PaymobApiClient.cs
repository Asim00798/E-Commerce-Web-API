#region Using Directives
using E_Commerce.Domain.SharedKernel.Services;
using E_Commerce.Infrastructure.Payment.Configuration;
using E_Commerce.Infrastructure.Payment.Providers.Paymob.Api.Models;
using E_Commerce.Infrastructure.Payment.Providers.Paymob.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
#endregion

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Api.Client;

/// <summary>
/// Paymob API client.
///
/// Endpoint paths:
///   - POST /v1/intention/                                      (Intention API, Bearer auth)
///   - POST /api/auth/tokens                                    (auth-token issuance)
///   - GET  /api/acceptance/transactions/{id}                   (Accept API — transaction inquiry)
///   - POST /api/acceptance/void_refund/refund                  (Accept API — refund)
///   - GET  /api/ecommerce/orders/transaction_inquiry           (Ecommerce API — merchant order lookup)
///
/// All requests use the Bearer auth token obtained from POST /api/auth/tokens.
/// If any Accept API endpoint rejects the Bearer token with 401 (some Paymob
/// tenants require <c>Authorization: Token {secret_key}</c> on the Accept API),
/// switch that method to <c>new AuthenticationHeaderValue("Token", _options.SecretKey)</c>
/// and drop the 401 retry for it.
/// </summary>
public sealed class PaymobApiClient
{
    private readonly HttpClient _httpClient;
    private readonly PaymobOptions _options;
    private readonly ILogger<PaymobApiClient> _logger;
    private readonly IClock _clock;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiresAt = DateTimeOffset.MinValue;
    private const int TokenExpiryBufferSeconds = 60;

    public PaymobApiClient(
        HttpClient httpClient,
        IOptions<PaymobOptions> options,
        ILogger<PaymobApiClient> logger,
        IClock clock)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _clock = clock;

        _httpClient.BaseAddress = BuildBaseAddress(_options.BaseUrl);
    }

    public async Task<CreateIntentionResponse> CreateIntentionAsync(
        CreateIntentionRequest request,
        CancellationToken ct)
    {
        using var response = await SendWithUnauthorizedRetryAsync(
            token => new HttpRequestMessage(HttpMethod.Post, "v1/intention/")
            {
                Content = JsonContent.Create(request),
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
            },
            ct);

        await EnsurePaymobSuccessAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<CreateIntentionResponse>(cancellationToken: ct)
               ?? throw new PaymobApiException(
                   response.StatusCode,
                   null,
                   "Paymob intention response was empty.");
    }

    /// <summary>
    /// Looks up a transaction by the merchant order ID. Used during payment
    /// reconciliation when a payment has not yet been captured and therefore
    /// has no provider transaction ID.
    /// </summary>
    public async Task<PaymobStatusResponse> GetTransactionStatusByMerchantOrderIdAsync(
        string merchantOrderId,
        CancellationToken ct)
    {
        var encoded = Uri.EscapeDataString(merchantOrderId);

        using var response = await SendWithUnauthorizedRetryAsync(
            token => new HttpRequestMessage(
                HttpMethod.Get,
                $"api/ecommerce/orders/transaction_inquiry?merchant_order_id={encoded}")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
            },
            ct);

        await EnsurePaymobSuccessAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<PaymobStatusResponse>(
                   cancellationToken: ct)
               ?? throw new PaymobApiException(
                   response.StatusCode,
                   null,
                   "Paymob merchant order inquiry response was empty.");
    }

    /// <summary>
    /// Transaction inquiry by provider transaction ID. Also serves refund
    /// status — refund outcome is read from the parent transaction's
    /// <c>is_refunded</c> field.
    /// </summary>
    public async Task<PaymobStatusResponse> GetTransactionStatusAsync(
        string providerTransactionId,
        CancellationToken ct)
    {
        using var response = await SendWithUnauthorizedRetryAsync(
            token => new HttpRequestMessage(
                HttpMethod.Get,
                $"api/acceptance/transactions/{Uri.EscapeDataString(providerTransactionId)}")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
            },
            ct);

        await EnsurePaymobSuccessAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<PaymobStatusResponse>(
                   cancellationToken: ct)
               ?? throw new PaymobApiException(
                   response.StatusCode,
                   null,
                   "Paymob transaction status response was empty.");
    }

    public async Task<PaymobRefundResponse> RefundAsync(
        string providerTransactionId,
        long amountInMinorUnit,
        CancellationToken ct)
    {
        var payload = new
        {
            transaction_id = providerTransactionId,
            amount_cents = amountInMinorUnit
        };

        using var response = await SendWithUnauthorizedRetryAsync(
            token => new HttpRequestMessage(HttpMethod.Post, "api/acceptance/void_refund/refund")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"),
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
            },
            ct);

        await EnsurePaymobSuccessAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<PaymobRefundResponse>(cancellationToken: ct)
               ?? throw new PaymobApiException(
                   response.StatusCode,
                   null,
                   "Paymob refund response was empty.");
    }

    #region Private Methods

    /// <summary>
    /// Normalises the configured base URL to a root URL ending in "/".
    /// Accepts either <c>https://accept.paymob.com</c> or
    /// <c>https://accept.paymob.com/api</c> — the trailing <c>/api</c> segment
    /// is stripped so relative paths can include <c>api/</c> where needed.
    /// </summary>
    private static Uri BuildBaseAddress(string configuredBaseUrl)
    {
        var baseUrl = configuredBaseUrl.TrimEnd('/');

        const string apiSuffix = "/api";
        if (baseUrl.EndsWith(apiSuffix, StringComparison.OrdinalIgnoreCase))
            baseUrl = baseUrl[..^apiSuffix.Length];

        return new Uri(baseUrl + "/");
    }

    private async Task<HttpResponseMessage> SendWithUnauthorizedRetryAsync(
        Func<string, HttpRequestMessage> requestFactory,
        CancellationToken ct)
    {
        var token = await GetTokenAsync(ct);
        var request = requestFactory(token);

        var response = await _httpClient.SendAsync(request, ct);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        response.Dispose();

        await InvalidateTokenAsync(token, ct);

        var newToken = await GetTokenAsync(ct);
        request = requestFactory(newToken);

        return await _httpClient.SendAsync(request, ct);
    }

    private async Task<string> GetTokenAsync(CancellationToken ct)
    {
        await _tokenLock.WaitAsync(ct);

        try
        {
            if (_cachedToken is null || _clock.UtcNow >= _tokenExpiresAt)
            {
                await RefreshTokenAsync(ct);
            }

            return _cachedToken!;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task RefreshTokenAsync(CancellationToken ct)
    {
        var payload = new { api_key = _options.ApiKey };

        using var response = await _httpClient.PostAsJsonAsync("api/auth/tokens", payload, ct);
        await EnsurePaymobSuccessAsync(response, ct);

        var auth = await response.Content.ReadFromJsonAsync<PaymobAuthResponse>(cancellationToken: ct);

        if (auth?.Token is null)
        {
            throw new PaymobApiException(
                response.StatusCode,
                null,
                "Paymob authentication returned no token.");
        }

        var lifetimeSeconds = auth.ExpiresInSeconds > 0
            ? auth.ExpiresInSeconds
            : 3600;

        _cachedToken = auth.Token;
        _tokenExpiresAt = _clock.UtcNow.AddSeconds(
            Math.Max(lifetimeSeconds - TokenExpiryBufferSeconds, 30));
    }

    private async Task InvalidateTokenAsync(string token, CancellationToken ct)
    {
        await _tokenLock.WaitAsync(ct);

        try
        {
            if (_cachedToken == token)
            {
                _cachedToken = null;
                _tokenExpiresAt = DateTimeOffset.MinValue;
            }
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task EnsurePaymobSuccessAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        var truncated = body.Length > 500 ? body[..500] : body;

        _logger.LogError(
            "Paymob API error {StatusCode}: {ResponseBody}",
            response.StatusCode,
            truncated);

        throw new PaymobApiException(
            response.StatusCode,
            truncated,
            "Paymob API request failed.");
    }

    #endregion
}