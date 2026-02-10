
namespace E_Commerce.Domain.Interfaces
{
    public interface ICurrentUser
    {
        Guid? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        IEnumerable<string> Roles { get; }
        bool IsAuthenticated { get; }
    }
}
