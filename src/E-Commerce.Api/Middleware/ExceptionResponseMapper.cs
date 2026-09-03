using System.Net;
using Microsoft.EntityFrameworkCore;
using E_Commerce.Application.Shared.Exceptions;

namespace E_Commerce.Api.Middleware;

/// <summary>
/// Default implementation that maps well‑known exception types
/// to appropriate HTTP response data.
/// </summary>
public sealed class ExceptionResponseMapper : IExceptionResponseMapper
{
    public (HttpStatusCode StatusCode, string Title, IDictionary<string, string[]>? Errors)
        Map(Exception exception) => exception switch
        {
            ValidationException validationEx =>
                (HttpStatusCode.BadRequest, "Validation Error", validationEx.Errors),

            // Use a dedicated Message property if you want to avoid leaking internal details.
            NotFoundException notFoundEx =>
                (HttpStatusCode.NotFound, notFoundEx.Message, null),

            ForbiddenAccessException =>
                (HttpStatusCode.Forbidden, "Forbidden Access", null),

            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "Unauthorized", null),

            DbUpdateConcurrencyException =>
                (HttpStatusCode.Conflict, "Concurrency Conflict", null),

            _ =>
                (HttpStatusCode.InternalServerError, "Internal Server Error", null)
        };
}