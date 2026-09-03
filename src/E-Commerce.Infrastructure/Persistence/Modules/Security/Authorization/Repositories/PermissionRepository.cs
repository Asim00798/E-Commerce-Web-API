using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Infrastructure.Security.Authorization.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Infrastructure.Persistence.Modules.Security.Authorization.Repositories;

/// <summary>
/// Repository for managing Permission entities.
/// </summary>
internal sealed class PermissionRepository
{
    private readonly AppDbContext _dbContext;

    public PermissionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbContext.Set<Permission>().FindAsync(new object[] { id }, ct);

    public async Task<Permission?> GetByNameAsync(string name, CancellationToken ct = default)
        => await _dbContext.Set<Permission>()
            .FirstOrDefaultAsync(p => p.Name == name, ct);

    public async Task<List<Permission>> GetAllAsync(CancellationToken ct = default)
        => await _dbContext.Set<Permission>().ToListAsync(ct);

    public async Task AddAsync(Permission permission, CancellationToken ct = default)
        => await _dbContext.Set<Permission>().AddAsync(permission, ct);

    public void Update(Permission permission)
        => _dbContext.Set<Permission>().Update(permission);

    public void Remove(Permission permission)
        => _dbContext.Set<Permission>().Remove(permission);
}