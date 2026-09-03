using E_Commerce.Application.Modules.Identity.AccountManagement.Dtos;

namespace E_Commerce.Application.Modules.Identity.AccountManagement.Abstractions;

/// <summary>
/// Reads account information and security state for the Identity module.
/// </summary>
public interface IAccountReader
{
    Task<AccountDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AccountDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<AccountSecurityDto?> GetSecurityAsync(Guid userId, CancellationToken cancellationToken = default);
}