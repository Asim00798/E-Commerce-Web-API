using E_Commerce.Application.Shared.Security.Authorization.Contracts;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Infrastructure.Security.Authorization.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Seeders;

/// <summary>
/// Reconciles the Permissions table with the permission constants declared by
/// every IPermissionSource implementation.
///
/// Fail-fast on duplicate permission identities across sources: two sources
/// declaring the same string is a configuration error, not a state to
/// reconcile.
///
/// Additive-only after validation: inserts permissions that exist in code but
/// not in the DB. Never deletes. Orphans (DB rows not in code) are counted
/// and logged.
///
/// Owns its own transaction. Called exclusively by StartupSeeder.
/// </summary>
public sealed class PermissionSeed
{
    private readonly AppDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IPermissionSource> _sources;
    private readonly ILogger<PermissionSeed> _logger;

    public PermissionSeed(
        AppDbContext db,
        IUnitOfWork unitOfWork,
        IEnumerable<IPermissionSource> sources,
        ILogger<PermissionSeed> logger)
    {
        _db = db;
        _unitOfWork = unitOfWork;
        _sources = sources;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        var declared = DiscoverDeclaredPermissions();
        EnsureNoDuplicates(declared);

        var existing = await LoadExistingPermissionNamesAsync(ct);

        var declaredSet = declared.ToHashSet(StringComparer.Ordinal);
        var existingSet = existing.ToHashSet(StringComparer.Ordinal);

        var missing = declared.Where(name => !existingSet.Contains(name)).ToList();
        var orphaned = existing.Count(name => !declaredSet.Contains(name));

        if (missing.Count > 0)
        {
            await InsertPermissionsAsync(missing, ct);
        }

        _logger.LogInformation(
            "Permissions: {Declared} declared, {Added} added, {Orphaned} orphaned",
            declared.Count,
            missing.Count,
            orphaned);
    }

    // ------------------------------------------------------------------
    // Discovery and validation
    // ------------------------------------------------------------------

    private List<string> DiscoverDeclaredPermissions() =>
        _sources.SelectMany(s => s.Describe()).ToList();

    /// <summary>
    /// Fail-fast: two sources declaring the same permission identity is a
    /// configuration error, not a runtime race. Detected before any
    /// deduplication so the error is not silently masked.
    /// </summary>
    private static void EnsureNoDuplicates(IReadOnlyList<string> declared)
    {
        var duplicates = declared
            .GroupBy(name => name, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new InvalidOperationException(
                $"Duplicate permission identities declared by permission sources: " +
                $"{string.Join(", ", duplicates)}");
        }
    }

    // ------------------------------------------------------------------
    // Persistence
    // ------------------------------------------------------------------

    private async Task<List<string>> LoadExistingPermissionNamesAsync(CancellationToken ct) =>
        await _db.Permissions
            .AsNoTracking()
            .Select(p => p.Name)
            .ToListAsync(ct);

    /// <summary>
    /// Inserts the given permission names in a single transaction.
    /// Rolls back on any failure.
    /// </summary>
    private async Task InsertPermissionsAsync(
        IReadOnlyList<string> names,
        CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            foreach (var name in names)
            {
                _db.Permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = name
                });
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}