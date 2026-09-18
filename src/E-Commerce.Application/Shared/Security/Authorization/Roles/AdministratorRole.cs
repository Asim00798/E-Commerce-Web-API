using E_Commerce.Application.Shared.Security.Authorization.Contracts;

namespace E_Commerce.Application.Shared.Security.Authorization.Roles;

public sealed class AdministratorRole : IRole
{
    public string Name => SystemRoles.Administrator;

    public IEnumerable<string> GetPermissions() => [IRole.AllPermissions];
}