using E_Commerce.Application.Shared.Security.Authorization.Contracts;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Infrastructure.Security.Authorization.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Seeders;

/// <summary>
/// Reconciles the RolePermissions table with the permissions declared by every
/// IRole implementation.
///
/// Validation is fail-fast and runs before any insert. It checks:
///   - duplicate role names across IRole implementations
///   - every permission name (or wildcard) resolves to a declared permission
///   - no role's expanded permission set contains duplicates
///
/// Wildcards ("*") expand against the current permission set on every run.
///
/// The unique-key catch on insert therefore unambiguously means "another
/// instance won the startup race", not "the declaration is malformed".
///
/// Additive-only: inserts pairs that exist in the declarations but not in the
/// DB. Never deletes.
/// </summary>
public sealed class RolePermissionSeed
{
    private readonly AppDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<IRole> _roles;
    private readonly IEnumerable<IPermissionSource> _permissionSources;
    private readonly ILogger<RolePermissionSeed> _logger;

    public RolePermissionSeed(
        AppDbContext db,
        IUnitOfWork unitOfWork,
        IEnumerable<IRole> roles,
        IEnumerable<IPermissionSource> permissionSources,
        ILogger<RolePermissionSeed> logger)
    {
        _db = db;
        _unitOfWork = unitOfWork;
        _roles = roles;
        _permissionSources = permissionSources;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        var allPermissions = DiscoverAllPermissions();
        var allPermissionsSet = allPermissions.ToHashSet(StringComparer.Ordinal);

        // Fail-fast: validate and expand role declarations before touching the DB.
        var expandedByRole = ValidateAndExpand(allPermissions, allPermissionsSet);

        var rolesByName = await LoadRolesByNameAsync(ct);
        var permissionsByName = await LoadPermissionsByNameAsync(ct);
        var existingPairs = await LoadExistingPairsAsync(ct);

        var toAdd = BuildMappingsToAdd(expandedByRole, rolesByName, permissionsByName, existingPairs);

        if (toAdd.Count == 0)
        {
            _logger.LogInformation("Role-permission mappings: no changes");
            return;
        }

        await InsertMappingsAsync(toAdd, ct);

        _logger.LogInformation(
            "Role-permission mappings: {Added} pairs added",
            toAdd.Count);
    }

    // ------------------------------------------------------------------
    // Discovery
    // ------------------------------------------------------------------

    private List<string> DiscoverAllPermissions() =>
        _permissionSources
            .SelectMany(s => s.Describe())
            .Distinct(StringComparer.Ordinal)
            .ToList();

    // ------------------------------------------------------------------
    // Validation and expansion
    // ------------------------------------------------------------------

    /// <summary>
    /// Validates every role declaration and expands wildcards.
    /// Throws on duplicate role names, unknown permission names, or
    /// duplicate permissions within a single role's expanded set.
    /// </summary>
    private Dictionary<string, List<string>> ValidateAndExpand(
        IReadOnlyList<string> allPermissions,
        IReadOnlySet<string> allPermissionsSet)
    {
        EnsureNoDuplicateRoles();

        var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var role in _roles)
        {
            result[role.Name] = ExpandRolePermissions(role, allPermissions, allPermissionsSet);
        }

        return result;
    }

    private void EnsureNoDuplicateRoles()
    {
        var duplicates = _roles
            .Select(r => r.Name)
            .GroupBy(n => n, StringComparer.Ordinal)
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

    /// <summary>
    /// Expands a single role's declared permissions, applying wildcard
    /// expansion and validating every name. Fail-fast on unknown names or
    /// duplicates within the role.
    /// </summary>
    private static List<string> ExpandRolePermissions(
        IRole role,
        IReadOnlyList<string> allPermissions,
        IReadOnlySet<string> allPermissionsSet)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var expanded = new List<string>();

        foreach (var name in role.GetPermissions())
        {
            if (name == IRole.AllPermissions)
            {
                ExpandWildcard(role, allPermissions, seen, expanded);
                continue;
            }

            EnsurePermissionExists(role, name, allPermissionsSet);
            EnsureNotDuplicate(role, name, seen);
            expanded.Add(name);
        }

        return expanded;
    }

    private static void ExpandWildcard(
        IRole role,
        IReadOnlyList<string> allPermissions,
        HashSet<string> seen,
        List<string> expanded)
    {
        foreach (var permission in allPermissions)
        {
            if (!seen.Add(permission))
            {
                throw new InvalidOperationException(
                    $"Role '{role.Name}' has duplicate permission '{permission}' " +
                    $"after wildcard expansion.");
            }
            expanded.Add(permission);
        }
    }

    private static void EnsurePermissionExists(
        IRole role,
        string permissionName,
        IReadOnlySet<string> allPermissionsSet)
    {
        if (!allPermissionsSet.Contains(permissionName))
        {
            throw new InvalidOperationException(
                $"Role '{role.Name}' references unknown permission '{permissionName}'.");
        }
    }

    private static void EnsureNotDuplicate(
        IRole role,
        string permissionName,
        HashSet<string> seen)
    {
        if (!seen.Add(permissionName))
        {
            throw new InvalidOperationException(
                $"Role '{role.Name}' declares duplicate permission '{permissionName}'.");
        }
    }

    // ------------------------------------------------------------------
    // Persistence reads
    // ------------------------------------------------------------------

    private async Task<Dictionary<string, Guid>> LoadRolesByNameAsync(CancellationToken ct) =>
        await _db.Roles
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Name!, r => r.Id, StringComparer.Ordinal, ct);

    private async Task<Dictionary<string, Guid>> LoadPermissionsByNameAsync(CancellationToken ct) =>
        await _db.Permissions
            .AsNoTracking()
            .ToDictionaryAsync(p => p.Name, p => p.Id, StringComparer.Ordinal, ct);

    private async Task<HashSet<(Guid RoleId, Guid PermissionId)>> LoadExistingPairsAsync(
        CancellationToken ct)
    {
        var pairs = await _db.RolePermissions
            .AsNoTracking()
            .Select(rp => new { rp.RoleId, rp.PermissionId })
            .ToListAsync(ct);

        return pairs.Select(p => (p.RoleId, p.PermissionId)).ToHashSet();
    }

    // ------------------------------------------------------------------
    // Diff computation
    // ------------------------------------------------------------------

    /// <summary>
    /// Computes the set of (RoleId, PermissionId) pairs that need to be inserted.
    /// Validation has already guaranteed that the desired pairs are internally
    /// unique within the declarations.
    /// </summary>
    private static List<RolePermission> BuildMappingsToAdd(
        Dictionary<string, List<string>> expandedByRole,
        Dictionary<string, Guid> rolesByName,
        Dictionary<string, Guid> permissionsByName,
        HashSet<(Guid RoleId, Guid PermissionId)> existingPairs)
    {
        var toAdd = new List<RolePermission>();

        foreach (var (roleName, permissionNames) in expandedByRole)
        {
            var roleId = rolesByName[roleName];

            foreach (var permissionName in permissionNames)
            {
                var permissionId = permissionsByName[permissionName];
                var pair = (roleId, permissionId);

                if (existingPairs.Contains(pair))
                    continue;

                toAdd.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
            }
        }

        return toAdd;
    }

    // ------------------------------------------------------------------
    // Persistence writes
    // ------------------------------------------------------------------

    /// <summary>
    /// Inserts the given mappings in a single transaction.
    ///
    /// A unique-constraint violation means another instance inserted the same
    /// pairs concurrently — declarations have already been validated, so this
    /// is not a malformed-map signal. Treated as success.
    /// </summary>
    private async Task InsertMappingsAsync(
        IReadOnlyList<RolePermission> mappings,
        CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            _db.RolePermissions.AddRange(mappings);
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogInformation(
                "Role-permission race detected — another instance synchronized the same pairs.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException sql
        && (sql.Number == 2601 || sql.Number == 2627);
}