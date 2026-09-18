using E_Commerce.Application.Shared.Security.Authorization.Roles;
using E_Commerce.Domain.SharedKernel.PersistenceAbstractions;
using E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Configuration;
using E_Commerce.Infrastructure.Security.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace E_Commerce.Infrastructure.Security.Authorization.DataSeeding.Seeders;

/// <summary>
/// Creates the seed administrator user when configured to do so.
///
/// Disabled by default. Enable only in development or as a one-time bootstrap
/// in a new environment, then disable again.
///
/// Runs in its own transaction so that user creation and role assignment are
/// atomic. A failure between the two operations does not leave an orphan user
/// without the Administrator role.
///
/// Assumes ASP.NET Core Identity's UserStore is registered against the same
/// scoped AppDbContext used by the UnitOfWork. In that setup, UserManager's
/// internal SaveChanges calls participate in the transaction opened here.
///
/// Create-if-absent semantics: if the configured admin already exists, nothing
/// changes — the seeder does not reconcile role membership on an existing user.
/// </summary>
public sealed class IdentitySeed
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptions<DataSeedingOptions> _options;
    private readonly ILogger<IdentitySeed> _logger;

    public IdentitySeed(
        UserManager<User> userManager,
        IUnitOfWork unitOfWork,
        IOptions<DataSeedingOptions> options,
        ILogger<IdentitySeed> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _options = options;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        var options = _options.Value;

        if (!options.CreateSeedAdmin)
        {
            _logger.LogInformation("Seed admin disabled — skipping identity seeding.");
            return;
        }

        ValidateConfiguration(options);

        if (await AdminAlreadyExistsAsync(options.AdminEmail))
        {
            _logger.LogInformation(
                "Seed admin already exists ({Email}) — skipping.",
                options.AdminEmail);
            return;
        }

        await CreateAdminAsync(options, ct);

        _logger.LogInformation(
            "Seed admin created ({Email}) with role {Role}.",
            options.AdminEmail,
            SystemRoles.Administrator);
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private static void ValidateConfiguration(DataSeedingOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.AdminEmail) ||
            string.IsNullOrWhiteSpace(options.AdminPassword))
        {
            throw new InvalidOperationException(
                "DataSeeding:CreateSeedAdmin is true but AdminEmail or AdminPassword is missing.");
        }
    }

    private async Task<bool> AdminAlreadyExistsAsync(string email)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        return existing is not null;
    }

    private async Task CreateAdminAsync(DataSeedingOptions options, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var user = await CreateUserAsync(options);
            await AssignAdministratorRoleAsync(user);

            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task<User> CreateUserAsync(DataSeedingOptions options)
    {
        var user = new User
        {
            UserName = options.AdminEmail,
            Email = options.AdminEmail,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, options.AdminPassword);
        EnsureSucceeded(result, "Failed to create seed admin");

        return user;
    }

    private async Task AssignAdministratorRoleAsync(User user)
    {
        var result = await _userManager.AddToRoleAsync(user, SystemRoles.Administrator);
        EnsureSucceeded(result, "Failed to assign Administrator role to seed admin");
    }

    private static void EnsureSucceeded(IdentityResult result, string failureMessage)
    {
        if (result.Succeeded)
            return;

        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException($"{failureMessage}: {errors}");
    }
}