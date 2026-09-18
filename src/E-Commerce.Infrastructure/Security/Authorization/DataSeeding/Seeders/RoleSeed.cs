using E_Commerce.Application.Shared.Security.Authorization.Contracts;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Seeders;

/// <summary>
/// Reconciles the AspNetRoles table with the roles declared by every
/// IRole implementation.
///
/// Fail-fast on duplicate role names across implementations. The declarations
/// are the source of truth for which roles exist; the DB is reconciled upward.
///
/// Uses RoleManager so Identity normalization (NormalizedName) is applied.
/// RoleManager.CreateAsync internally invokes SaveChangesAsync on the Identity
/// store's DbContext. As long as that store is registered against the same
/// scoped AppDbContext as the UnitOfWork, its writes participate in the
/// transaction opened here.
///
/// Additive-only: inserts roles that exist in code but not in the DB.
/// Never deletes. Orphans are counted and logged.
/// </summary>
public sealed class RoleSeed
{
    private readonly AppDbContext _db;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IRole> _roles;
    private readonly ILogger<RoleSeed> _logger;

    public RoleSeed(
        AppDbContext db,
        RoleManager<IdentityRole<Guid>> roleManager,
        IUnitOfWork unitOfWork,
        IEnumerable<IRole> roles,
        ILogger<RoleSeed> logger)
    {
        _db = db;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _roles = roles;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        var declared = DiscoverDeclaredRoles();
        EnsureNoDuplicates(declared);

        var existing = await LoadExistingRoleNamesAsync(ct);

        var declaredSet = declared.ToHashSet(StringComparer.Ordinal);
        var existingSet = existing.ToHashSet(StringComparer.Ordinal);

        var missing = declared.Where(name => !existingSet.Contains(name)).ToList();
        var orphaned = existing.Count(name => !declaredSet.Contains(name));

        if (missing.Count > 0)
        {
            await InsertRolesAsync(missing, ct);
        }

        _logger.LogInformation(
            "Roles: {Declared} declared, {Added} added, {Orphaned} orphaned",
            declared.Count,
            missing.Count,
            orphaned);
    }

    // ------------------------------------------------------------------
    // Discovery and validation
    // ------------------------------------------------------------------

    private List<string> DiscoverDeclaredRoles() =>
        _roles.Select(r => r.Name).ToList();

    /// <summary>
    /// Fail-fast: two role classes declaring the same role identity is a
    /// configuration error, not a state to reconcile.
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
                $"Duplicate role identities declared by role classes: " +
                $"{string.Join(", ", duplicates)}");
        }
    }

    // ------------------------------------------------------------------
    // Persistence
    // ------------------------------------------------------------------

    private async Task<List<string>> LoadExistingRoleNamesAsync(CancellationToken ct) =>
        await _db.Roles
            .AsNoTracking()
            .Select(r => r.Name!)
            .ToListAsync(ct);

    /// <summary>
    /// Creates the given roles in a single transaction using RoleManager.
    /// Rolls back on any failure.
    /// </summary>
    private async Task InsertRolesAsync(
        IReadOnlyList<string> names,
        CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            foreach (var name in names)
            {
                await CreateRoleAsync(name);
            }

            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task CreateRoleAsync(string name)
    {
        var role = new IdentityRole<Guid>
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create role '{name}': {errors}");
        }
    }
}