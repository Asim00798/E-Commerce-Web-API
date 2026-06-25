using E_Commerce.Infrastructure.Persistence.Context;
using E_Commerce.Application.Shared.Identity;

namespace E_Commerce.Infrastructure.Identity.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly AppDbContext _db;

        public AuthorizationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string permission)
        {
            var hasPermission =
                await (from ur in _db.UserRoles
                       join rp in _db.RolePermissions on ur.RoleId equals rp.RoleId
                       join p in _db.Permissions on rp.PermissionId equals p.Id
                       where ur.UserId == userId && p.Name == permission
                       select p)
                .AnyAsync();

            return hasPermission;
        }
    }
}
