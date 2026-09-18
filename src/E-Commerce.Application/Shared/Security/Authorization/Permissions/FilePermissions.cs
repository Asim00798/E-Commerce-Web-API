using E_Commerce.Application.Shared.Security.Authorization.Contracts;

namespace E_Commerce.Application.Shared.Security.Authorization.Permissions;

public sealed class FilePermissions : IPermissionSource
{
    public const string Read = "Files.Read";
    public const string Upload = "Files.Upload";
    public const string Delete = "Files.Delete";
    public const string Manage = "Files.Manage";
}