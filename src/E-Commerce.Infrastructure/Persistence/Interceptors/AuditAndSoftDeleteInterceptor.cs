using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Infrastructure.Persistence.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace E_Commerce.Infrastructure.Persistence.Interceptors;

public sealed class AuditAndSoftDeleteInterceptor : SaveChangesInterceptor
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

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

        /*
         * Capture the original Deleted state BEFORE soft-delete
         * changes Deleted → Modified.
         */
        var originallyDeletedEntries = context.ChangeTracker
            .Entries()
            .Where(entry => entry.State == EntityState.Deleted)
            .ToHashSet();

        /*
         * Convert physical DELETE requests into soft deletes.
         */
        context.ApplySoftDelete();

        /*
         * Capture request metadata.
         */
        var ipAddress = GetIpAddress();

        var correlationId = GetCorrelationId();

        /*
         * Create audit records.
         *
         * Soft-deleted entities remain semantically Deleted
         * even though EF ultimately persists them using UPDATE.
         */
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

    private string? GetIpAddress()
    {
        return _httpContextAccessor.HttpContext?
            .Connection
            .RemoteIpAddress?
            .ToString();
    }

    private Guid? GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
            return null;

        var correlationIdValue =
            httpContext.Request.Headers[CorrelationIdHeader]
                .FirstOrDefault();

        return Guid.TryParse(
            correlationIdValue,
            out var correlationId)
                ? correlationId
                : null;
    }
}