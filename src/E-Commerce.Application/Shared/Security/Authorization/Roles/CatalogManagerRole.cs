using E_Commerce.Application.Shared.Security.Authorization.Contracts;
using E_Commerce.Application.Shared.Security.Authorization.Permissions;

namespace E_Commerce.Application.Shared.Security.Authorization.Roles;

public sealed class CatalogManagerRole : IRole
{
    public string Name => SystemRoles.CatalogManager;

    public IEnumerable<string> GetPermissions() =>
    [
        CatalogPermissions.ViewProducts,
        CatalogPermissions.ManageProducts,
        CatalogPermissions.ManageBrands,
        CatalogPermissions.ManageCategories
    ];
}