using E_Commerce.Application.Shared.Security.Cryptography;
using E_Commerce.Infrastructure.Security.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Infrastructure.Security.Cryptography;

/// <summary>
/// Implements <see cref="IPasswordHasher"/> by delegating to ASP.NET Core Identity's
/// <see cref="IPasswordHasher{TUser}"/> for the application's <see cref="User"/> entity.
/// </summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    private readonly IPasswordHasher<User> _identityHasher;

    public PasswordHasher(IPasswordHasher<User> identityHasher)
    {
        _identityHasher = identityHasher;
    }

    /// <inheritdoc />
    public string HashPassword(string plainPassword)
    {
        var tempUser = new User();
        return _identityHasher.HashPassword(tempUser, plainPassword);
    }
}