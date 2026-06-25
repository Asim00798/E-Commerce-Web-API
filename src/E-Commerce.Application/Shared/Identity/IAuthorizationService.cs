namespace E_Commerce.Application.Shared.Identity
{
    public interface IAuthorizationService
    {
        Task<bool> HasPermissionAsync(Guid userId, string permission);
    }
}
