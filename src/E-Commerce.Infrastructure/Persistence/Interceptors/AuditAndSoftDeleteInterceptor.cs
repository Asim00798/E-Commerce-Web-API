using E_Commerce.Application.Shared.Constants;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Infrastructure.Persistence.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace E_Commerce.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Intercepts SaveChanges to:
/// <list type="bullet">
///   <item>Convert physical deletes into soft deletes.</item>
///   <item>Write audit log entries for created, modified, and soft-deleted entities.</item>
///   <item>Capture the request's correlation ID and client IP on each audit entry.</item>
/// </list>
///
/// The interceptor reads request metadata from <see cref="IHttpContextAccessor"/>.
/// When no HTTP context exists (background jobs, seeding), the metadata is null —
/// audit entries are still written, just without correlation ID or IP.
/// </summary>
public sealed class AuditAndSoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser? _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditAndSoftDeleteInterceptor(
        IHttpContextAccessor httpContextAccessor,
        ICurrentUser? currentUser = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _currentUser = currentUser;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;

        if (context is null)
        {
            return base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        // Capture the original Deleted state BEFORE soft-delete converts
        // Deleted → Modified. Once ApplySoftDelete runs, the signal is lost.
        var originallyDeletedEntries = context.ChangeTracker
            .Entries()
            .Where(entry => entry.State == EntityState.Deleted)
            .ToHashSet();

        // Convert physical DELETE requests into soft deletes.
        context.ApplySoftDelete();

        // Capture request-scoped metadata.
        // Both reads return null when there is no HttpContext (background jobs, seeding).
        var ipAddress = GetIpAddress();
        var correlationId = GetCorrelationId();

        // Create audit records.
        // Soft-deleted entities remain semantically Deleted even though
        // EF ultimately persists them using UPDATE.
        context.ApplyAuditLogging(
            currentUserId: _currentUser?.UserId,
            originallyDeletedEntries: originallyDeletedEntries,
            ipAddress: ipAddress,
            correlationId: correlationId);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    /// <summary>
    /// Reads the client IP from the current HTTP connection.
    /// When ForwardedHeaders middleware is active and the request arrives via
    /// a trusted proxy, this value reflects the real client IP rather than the
    /// proxy's address.
    /// </summary>
    private string? GetIpAddress()
    {
        return _httpContextAccessor.HttpContext?
            .Connection
            .RemoteIpAddress?
            .ToString();
    }

    /// <summary>
    /// Reads the correlation ID from <see cref="HttpContext.Items"/>, where
    /// <c>CorrelationIdMiddleware</c> stores it at the start of the request.
    ///
    /// The request header is <b>not</b> read directly: the middleware generates
    /// a fresh ID when the client does not supply one, so the header may be
    /// absent even though a valid correlation ID exists for the request. Reading
    /// from Items gives the authoritative value in both cases.
    ///
    /// Returns null when there is no HTTP context (background jobs, seeding) or
    /// when the stored value is not a valid GUID.
    ///
    /// Note: the correlation middleware accepts any non-empty string from the
    /// client. If you want audit entries to preserve non-GUID correlation IDs,
    /// change the audit log entity's CorrelationId column to <c>string?</c> and
    /// return the raw string here instead of parsing.
    /// </summary>
    private Guid? GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
            return null;

        if (!httpContext.Items.TryGetValue(ContextKeys.CorrelationId, out var value)
            || value is not string correlationIdString
            || string.IsNullOrWhiteSpace(correlationIdString))
        {
            return null;
        }

        return Guid.TryParse(correlationIdString, out var correlationId)
            ? correlationId
            : null;
    }
}