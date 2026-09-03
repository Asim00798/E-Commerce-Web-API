using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Infrastructure.Security.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Infrastructure.Security.Identity.Services;

internal sealed class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<User> userManager,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Guid> CreateUserWithPrehashedPasswordAsync(
        CreateIdentityUserRequest request,
        CancellationToken ct = default)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Username ?? request.Email,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true,   // already verified
            PhoneNumberConfirmed = true,
            PasswordHash = request.PasswordHash   // pre‑hashed with IPasswordHasher
        };

        // UserManager.CreateAsync(user) persists the user as‑is,
        // running validators, normalisation, and security stamp updates,
        // but NOT re‑hashing the password.
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user {Email}: {Errors}", request.Email, errors);
            throw new IdentityOperationException(errors);
        }

        _logger.LogInformation("IdentityUser {UserId} created for {Email}", user.Id, request.Email);
        return user.Id;
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new IdentityOperationException($"User {userId} not found.");
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default)
        => await _userManager.FindByEmailAsync(normalizedEmail) != null;

    public async Task<bool> ExistsByPhoneAsync(string normalizedPhone, CancellationToken ct = default)
        => await _userManager.Users.AnyAsync(u => u.PhoneNumber == normalizedPhone, ct);

    public async Task<bool> ExistsByUsernameAsync(string normalizedUsername, CancellationToken ct = default)
        => await _userManager.FindByNameAsync(normalizedUsername) != null;
}