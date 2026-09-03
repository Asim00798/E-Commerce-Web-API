using System.Net;

namespace E_Commerce.Infrastructure.Payment.Providers.Paymob.Exceptions;

/// <summary>
/// Represents an error returned by the Paymob API.
/// Used only inside the Paymob Infrastructure adapter.
/// Inherits <see cref="HttpRequestException"/> so Application can treat it
/// as a transport/infrastructure failure without referencing Paymob.
/// </summary>
public sealed class PaymobApiException : HttpRequestException
{
    /// <summary>
    /// Truncated response body for diagnostics.
    /// Never expose this through an API response.
    /// </summary>
    public string? ResponseBody { get; }

    public PaymobApiException(
        HttpStatusCode statusCode,
        string? responseBody,
        string message)
        : base(message, null, statusCode)
    {
        ResponseBody = string.IsNullOrWhiteSpace(responseBody) ? "No response body." : responseBody;
    }
}