using System.Net;

namespace E_Commerce.Api.Middleware;

/// <summary>
/// Maps an exception to an HTTP status code, a user‑friendly title,
/// and an optional dictionary of validation errors.
/// This abstraction belongs to the API layer because it deals with HTTP semantics.
/// </summary>
public interface IExceptionResponseMapper
{
    (HttpStatusCode StatusCode, string Title, IDictionary<string, string[]>? Errors)
        Map(Exception exception);
}