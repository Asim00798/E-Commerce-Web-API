using E_Commerce.Application.Shared.Time;
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

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Api.Client;

public sealed class PaymobApiClient
{
    private readonly HttpClient _httpClient;
    private readonly PaymobOptions _options;
    private readonly ILogger<PaymobApiClient> _logger;
    private readonly IDateTime _dateTime;

    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiresAt = DateTimeOffset.MinValue;

    private const int TokenExpiryBufferSeconds = 60;

    public PaymobApiClient(
        HttpClient httpClient,
        IOptions<PaymobOptions> options,
        ILogger<PaymobApiClient> logger,
        IDateTime dateTime)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _dateTime = dateTime;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
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

    public async Task<PaymobStatusResponse> GetTransactionStatusAsync(
        string providerTransactionId,
        CancellationToken ct)
    {
        using var response = await SendWithUnauthorizedRetryAsync(
            token => new HttpRequestMessage(
                HttpMethod.Get,
                $"v1/transactions/{Uri.EscapeDataString(providerTransactionId)}")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
            },
            ct);

        await EnsurePaymobSuccessAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<PaymobStatusResponse>(cancellationToken: ct)
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
            amount = amountInMinorUnit
        };

        using var response = await SendWithUnauthorizedRetryAsync(
            token => new HttpRequestMessage(HttpMethod.Post, "v1/transactions/refunds")
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

        // Dispose the 401 response before retrying.
        response.Dispose();

        // Invalidate only the token that actually failed.
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
            if (_cachedToken is null || _dateTime.UtcNow >= _tokenExpiresAt)
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

        using var response = await _httpClient.PostAsJsonAsync("auth/tokens", payload, ct);
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
        _tokenExpiresAt = _dateTime.UtcNow.AddSeconds(
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

    public async Task<PaymobRefundStatusResponse> GetRefundStatusAsync(
     string refundTransactionId,
     CancellationToken ct)
    {
        var token = await GetTokenAsync(ct);

        using var requestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            $"v1/transactions/{Uri.EscapeDataString(refundTransactionId)}");

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(requestMessage, ct);
        await EnsurePaymobSuccessAsync(response, ct);

        return await response.Content.ReadFromJsonAsync<PaymobRefundStatusResponse>(cancellationToken: ct)
               ?? throw new PaymobApiException(
                   response.StatusCode,
                   null,
                   "Paymob refund status response was empty.");
    }
}